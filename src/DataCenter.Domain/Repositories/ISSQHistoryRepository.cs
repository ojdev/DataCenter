using DataCenter.Domain.AggregatesModel;

namespace DataCenter.Domain.Repositories;

/// <summary>
/// 双色球历史数据仓储接口
/// </summary>
public interface ISSQHistoryRepository
{
    /// <summary>
    /// 获取最新的双色球历史记录
    /// </summary>
    /// <returns>最新的双色球历史记录</returns>
    Task<SSQHistory?> GetLatestAsync();

    /// <summary>
    /// 批量添加双色球历史记录
    /// </summary>
    /// <param name="ssqHistories">双色球历史记录列表</param>
    /// <returns>任务</returns>
    Task AddRangeAsync(IEnumerable<SSQHistory> ssqHistories);

    /// <summary>
    /// 获取记录总数
    /// </summary>
    /// <returns>记录总数</returns>
    Task<int> CountAsync();
}