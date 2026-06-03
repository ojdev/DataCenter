namespace DataCenter.Application.Services;

/// <summary>
/// 双色球数据采集应用服务实现
/// </summary>
public class SSQCollectService : ISSQCollectService
{
    private readonly ISSQHttpCollectUtility _collectUtility;
    private readonly ILogger<SSQCollectService> _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="collectUtility">HTTP采集工具</param>
    /// <param name="logger">日志记录器</param>
    public SSQCollectService(ISSQHttpCollectUtility collectUtility, ILogger<SSQCollectService> logger)
    {
        _collectUtility = collectUtility ?? throw new ArgumentNullException(nameof(collectUtility));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 获取最新一期开奖期号
    /// </summary>
    /// <returns>最新期号</returns>
    public async Task<string> GetLatestPeriodicalNOAsync()
    {
        try
        {
            var periodicalNO = await _collectUtility.GetLastPeriodicalNOAsync();
            _logger.LogInformation("成功获取最新期号: {PeriodicalNO}", periodicalNO);
            return periodicalNO;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取最新期号失败");
            throw;
        }
    }

    /// <summary>
    /// 采集并保存最新开奖数据
    /// </summary>
    /// <returns>采集的记录数</returns>
    public async Task<int> CollectAndSaveLatestAsync()
    {
        try
        {
            var count = await _collectUtility.CheckAndAppendAsync();
            _logger.LogInformation("成功采集并保存 {Count} 条记录", count);
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "采集并保存最新数据失败");
            throw;
        }
    }

    /// <summary>
    /// 采集指定范围的开奖数据
    /// </summary>
    /// <param name="startPeriodicalNO">起始期号</param>
    /// <param name="endPeriodicalNO">结束期号</param>
    /// <returns>采集的记录数</returns>
    public async Task<int> CollectRangeAsync(string startPeriodicalNO, string endPeriodicalNO)
    {
        try
        {
            var count = await _collectUtility.CollectAsync(startPeriodicalNO, endPeriodicalNO);
            _logger.LogInformation("成功采集 {Count} 条记录，范围: {Start} - {End}", count, startPeriodicalNO, endPeriodicalNO);
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "采集指定范围数据失败: {Start} - {End}", startPeriodicalNO, endPeriodicalNO);
            throw;
        }
    }
}