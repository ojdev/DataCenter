namespace DataCenter.Infrastructure.Repositories;

/// <summary>
/// 双色球历史数据仓储数据库实现
/// </summary>
public class SSQHistoryRepository : ISSQHistoryRepository
{
    private readonly DataCenterDbContext _context;
    private readonly ILogger<SSQHistoryRepository> _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="context">数据库上下文</param>
    /// <param name="logger">日志记录器</param>
    public SSQHistoryRepository(DataCenterDbContext context, ILogger<SSQHistoryRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 获取最新的双色球历史记录
    /// </summary>
    /// <returns>最新的双色球历史记录</returns>
    public async Task<SSQHistory?> GetLatestAsync()
    {
        try
        {
            return await _context.SSQHistories
                .OrderByDescending(h => h.PeriodicalNO)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取最新历史记录失败");
            throw;
        }
    }

    /// <summary>
    /// 批量添加双色球历史记录
    /// </summary>
    /// <param name="ssqHistories">双色球历史记录列表</param>
    /// <returns>任务</returns>
    public async Task AddRangeAsync(IEnumerable<SSQHistory> ssqHistories)
    {
        if (ssqHistories == null)
            throw new ArgumentNullException(nameof(ssqHistories));

        try
        {
            var histories = ssqHistories.ToList();
            if (!histories.Any())
            {
                _logger.LogInformation("没有需要添加的历史记录");
                return;
            }

            var periodicalNOs = histories.Select(h => h.PeriodicalNO).ToList();

            var existingPeriodicalNOs = await _context.SSQHistories
                .Where(h => periodicalNOs.Contains(h.PeriodicalNO))
                .Select(h => h.PeriodicalNO)
                .ToListAsync();

            var newHistories = histories
                .Where(h => !existingPeriodicalNOs.Contains(h.PeriodicalNO))
                .ToList();

            if (newHistories.Any())
            {
                await _context.SSQHistories.AddRangeAsync(newHistories);
                await _context.SaveChangesAsync();
                _logger.LogInformation("成功批量添加 {Count} 条双色球历史记录", newHistories.Count);
            }
            else
            {
                _logger.LogInformation("所有历史记录已存在，无需添加");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量添加双色球历史记录失败");
            throw;
        }
    }

    /// <summary>
    /// 获取记录总数
    /// </summary>
    /// <returns>记录总数</returns>
    public async Task<int> CountAsync()
    {
        try
        {
            return await _context.SSQHistories.CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取记录总数失败");
            throw;
        }
    }
}