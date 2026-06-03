namespace DataCenter.Application.Utility;

/// <summary>
/// Http采集工具接口
/// </summary>
public interface ISSQHttpCollectUtility
{
    /// <summary>
    /// 本次是否追加了数据
    /// </summary>
    /// <returns>追加的数据数量</returns>
    Task<int> CheckAndAppendAsync();

    /// <summary>
    /// 开始进行采集
    /// </summary>
    /// <param name="startPeriodicalNO">起始期号</param>
    /// <param name="periodicalNO">结束期号</param>
    /// <returns>采集的数据数量</returns>
    Task<int> CollectAsync(string startPeriodicalNO = "03001", string periodicalNO = "");

    /// <summary>
    /// 获取最新一期彩票开奖期数
    /// </summary>
    /// <returns>最新期号</returns>
    Task<string> GetLastPeriodicalNOAsync();

    /// <summary>
    /// 获取最新一期开奖数据
    /// </summary>
    /// <returns>双色球历史数据对象</returns>
    Task<SSQHistory?> GetLatestDrawDataAsync();

    /// <summary>
    /// 采集指定范围的开奖数据
    /// </summary>
    /// <param name="startPeriodicalNO">起始期号</param>
    /// <param name="endPeriodicalNO">结束期号</param>
    /// <returns>双色球历史数据列表</returns>
    Task<IEnumerable<SSQHistory>> CollectRangeDataAsync(string startPeriodicalNO, string endPeriodicalNO);
}
