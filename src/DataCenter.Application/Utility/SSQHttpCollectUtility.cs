namespace DataCenter.Application.Utility;

/// <summary>
/// Http采集工具实现
/// </summary>
public class SSQHttpCollectUtility : ISSQHttpCollectUtility, IDisposable
{
    private readonly ILogger<SSQHttpCollectUtility> _logger;
    private readonly ISSQHistoryRepository _repository;
    private readonly HttpClient _httpClient;
    private readonly HttpClientHandler _handler;
    private bool _disposed;
    private int _idCounter = 1;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="logger">日志记录器</param>
    /// <param name="repository">双色球历史数据仓储</param>
    public SSQHttpCollectUtility(
        ILogger<SSQHttpCollectUtility> logger,
        ISSQHistoryRepository repository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));

        _handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
            AllowAutoRedirect = true,
            MaxAutomaticRedirections = 5,
            UseCookies = true,
            CookieContainer = new CookieContainer()
        };

        _httpClient = new HttpClient(_handler)
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        // 添加常见的浏览器请求头，模拟真实浏览器访问
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        _httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
        _httpClient.DefaultRequestHeaders.Add("Accept-Language", "zh-CN,zh;q=0.9,en;q=0.8");
        _httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
        _httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
        _httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
    }

    /// <summary>
    /// 获取最新一期彩票开奖期数
    /// </summary>
    /// <returns>最新期号</returns>
    public async Task<string> GetLastPeriodicalNOAsync()
    {
        const string url = "https://zx.500.com/static/info/kaijiang/xml/ssq/index.xml";
        const int maxRetries = 3;
        int retryCount = 0;

        while (retryCount < maxRetries)
        {
            try
            {
                retryCount++;
                _logger.LogInformation("尝试获取最新期号（第 {RetryCount} 次）", retryCount);

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Referer", "https://zx.500.com/");

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var xmlContent = await response.Content.ReadAsStringAsync();
                HtmlDocument doc = new();
                doc.LoadHtml(xmlContent);

                var periodicalNO = doc.DocumentNode.SelectSingleNode("//xml//periodicalno")?.InnerText ?? string.Empty;
                _logger.LogInformation("成功获取最新期号: {PeriodicalNO}", periodicalNO);

                return periodicalNO;
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                _logger.LogWarning("获取最新期号超时（第 {RetryCount}/{MaxRetries}）", retryCount, maxRetries);
                if (retryCount >= maxRetries)
                {
                    _logger.LogError(ex, "获取最新期号失败，已达到最大重试次数");
                    throw;
                }
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryCount)));
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning("获取最新期号失败（第 {RetryCount}/{MaxRetries}）: {Message}", retryCount, maxRetries, ex.Message);
                if (retryCount >= maxRetries)
                {
                    _logger.LogError(ex, "获取最新期号失败，已达到最大重试次数");
                    throw;
                }
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryCount)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取最新期号时发生未知错误");
                throw;
            }
        }

        throw new InvalidOperationException("获取最新期号失败");
    }

    /// <summary>
    /// 获取最新一期开奖数据
    /// </summary>
    /// <returns>双色球历史数据对象</returns>
    public async Task<SSQHistory?> GetLatestDrawDataAsync()
    {
        try
        {
            var latestNO = await GetLastPeriodicalNOAsync();
            var data = await CollectRangeDataAsync(latestNO, latestNO);
            return data.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取最新开奖数据失败");
            throw;
        }
    }

    /// <summary>
    /// 开始进行采集
    /// </summary>
    /// <param name="startPeriodicalNO">起始期号</param>
    /// <param name="periodicalNO">结束期号</param>
    /// <returns>采集的数据数量</returns>
    public async Task<int> CollectAsync(string startPeriodicalNO = "03001", string periodicalNO = "")
    {
        try
        {
            var endNO = string.IsNullOrEmpty(periodicalNO) ? await GetLastPeriodicalNOAsync() : periodicalNO;
            var data = await CollectRangeDataAsync(startPeriodicalNO, endNO);
            var count = data.Count();

            if (count > 0)
            {
                await _repository.AddRangeAsync(data);
                _logger.LogInformation("成功采集并保存 {Count} 条历史记录", count);
            }

            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "采集数据失败");
            throw;
        }
    }

    /// <summary>
    /// 采集指定范围的开奖数据
    /// 使用新链接: https://datachart.500.com/ssq/history/newinc/outball.php?start=03001&end={latest}
    /// </summary>
    /// <param name="startPeriodicalNO">起始期号</param>
    /// <param name="endPeriodicalNO">结束期号</param>
    /// <returns>双色球历史数据列表</returns>
    public async Task<IEnumerable<SSQHistory>> CollectRangeDataAsync(string startPeriodicalNO, string endPeriodicalNO)
    {
        string url = $"https://datachart.500.com/ssq/history/newinc/outball.php?start={startPeriodicalNO}&end={endPeriodicalNO}";

        try
        {
            var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, url));
            response.EnsureSuccessStatusCode();

            var data = await response.Content.ReadAsStringAsync();
            HtmlDocument doc = new();
            doc.LoadHtml(data);

            // 查找ID为tablelist的表格
            var table = doc.DocumentNode.Descendants("table").FirstOrDefault(t => t.GetAttributeValue("id", string.Empty) == "tablelist");
            if (table == null)
            {
                _logger.LogWarning("未找到数据表格");
                return Enumerable.Empty<SSQHistory>();
            }

            // 查找ID为tdata的tbody
            var tbody = table.Descendants("tbody").FirstOrDefault(t => t.GetAttributeValue("id", string.Empty) == "tdata");
            if (tbody == null)
            {
                _logger.LogWarning("未找到数据tbody");
                return Enumerable.Empty<SSQHistory>();
            }

            var results = new List<SSQHistory>();

            foreach (var tr in tbody.Descendants("tr"))
            {
                var td = tr.Descendants("td").ToArray();
                // 每行应有15列：期号、日期、6个出球顺序、6个红球、1个蓝球
                if (td.Length < 15) continue;

                // 期号（第1列）
                var periodicalNO = td[0].InnerText.Trim();
                if (string.IsNullOrEmpty(periodicalNO)) continue;

                // 开奖日期（第2列）
                var dateStr = td[1].InnerText.Trim();
                if (!DateTime.TryParse(dateStr, out DateTime drawDate))
                {
                    _logger.LogWarning("无法解析开奖日期: {DateStr}", dateStr);
                    continue;
                }

                // 出球顺序（第3-8列，共6个红球的摇出顺序）
                var outBallOrderList = new List<string>();
                for (int i = 2; i <= 7; i++)
                {
                    var ball = td[i].InnerText.Trim();
                    if (!string.IsNullOrEmpty(ball))
                    {
                        outBallOrderList.Add(ball.PadLeft(2, '0'));
                    }
                }
                var outBallOrder = string.Join(",", outBallOrderList);

                // 红球号码（第9-14列，共6个红球，已按大小顺序）
                var redBallsList = new List<string>();
                for (int i = 8; i <= 13; i++)
                {
                    var ball = td[i].InnerText.Trim();
                    if (!string.IsNullOrEmpty(ball))
                    {
                        redBallsList.Add(ball.PadLeft(2, '0'));
                    }
                }
                var redBalls = string.Join(",", redBallsList);

                // 蓝球号码（第15列）
                var blueBall = td[14].InnerText.Trim().PadLeft(2, '0');

                // 验证数据完整性
                if (outBallOrderList.Count == 6 && redBallsList.Count == 6 && !string.IsNullOrEmpty(blueBall))
                {
                    var history = new SSQHistory(
                        id: _idCounter++,
                        periodicalNO: periodicalNO,
                        drawDate: drawDate,
                        outBallOrder: outBallOrder,
                        redBalls: redBalls,
                        blueBall: blueBall
                    );
                    results.Add(history);
                }
                else
                {
                    _logger.LogWarning("数据不完整，跳过: 期号={PeriodicalNO}, 出球顺序={OutBallOrder}, 红球={RedBalls}, 蓝球={BlueBall}",
                        periodicalNO, outBallOrder, redBalls, blueBall);
                }
            }

            _logger.LogInformation("成功采集 {Count} 条数据，范围: {Start} - {End}", results.Count, startPeriodicalNO, endPeriodicalNO);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "采集指定范围数据失败: {Start} - {End}", startPeriodicalNO, endPeriodicalNO);
            throw;
        }
    }

    /// <summary>
    /// 本次是否追加了数据
    /// </summary>
    /// <returns>追加的数据数量</returns>
    public async Task<int> CheckAndAppendAsync()
    {
        try
        {
            var periodicalNO = await GetLastPeriodicalNOAsync();
            var latestHistory = await _repository.GetLatestAsync();

            if (latestHistory == null || latestHistory.PeriodicalNO != periodicalNO)
            {
                var startNO = latestHistory?.PeriodicalNO ?? "03001";
                var appendCount = await CollectAsync(startNO, periodicalNO);
                _logger.LogInformation("本次检查并追加了: {Count} 条历史记录", appendCount);
                return appendCount;
            }

            _logger.LogInformation("数据库已是最新，无需追加");
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查并追加数据失败");
            throw;
        }
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    /// <param name="disposing">是否正在释放托管资源</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _httpClient?.Dispose();
                _handler?.Dispose();
            }
            _disposed = true;
        }
    }
}

/// <summary>
/// HtmlAgilityPack扩展
/// </summary>
public static class HtmlAgilityPackEx
{
    /// <summary>
    /// 获取HtmlNode的class属性值
    /// </summary>
    /// <param name="htmlNode">HTML节点</param>
    /// <returns>class属性值</returns>
    public static string GetClassName(this HtmlNode htmlNode)
    {
        return htmlNode.GetAttributeValue<string>("class", string.Empty);
    }
}