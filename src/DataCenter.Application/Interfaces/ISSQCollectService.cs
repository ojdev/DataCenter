namespace DataCenter.Application.Interfaces;

/// <summary>
/// 双色球数据采集应用服务接口
/// </summary>
public interface ISSQCollectService
{
    /// <summary>
    /// 获取最新一期开奖期号
    /// </summary>
    /// <returns>最新期号</returns>
    Task<string> GetLatestPeriodicalNOAsync();

    /// <summary>
    /// 采集并保存最新开奖数据
    /// </summary>
    /// <returns>采集的记录数</returns>
    Task<int> CollectAndSaveLatestAsync();

    /// <summary>
    /// 采集指定范围的开奖数据
    /// </summary>
    /// <param name="startPeriodicalNO">起始期号</param>
    /// <param name="endPeriodicalNO">结束期号</param>
    /// <returns>采集的记录数</returns>
    Task<int> CollectRangeAsync(string startPeriodicalNO, string endPeriodicalNO);
}