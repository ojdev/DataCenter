using DataCenter.Domain.SeedWork;

namespace DataCenter.Domain.AggregatesModel;

/// <summary>
/// 双色球历史数据聚合根
/// </summary>
public class SSQHistory : AggregateRoot<int>
{
    /// <summary>
    /// 开奖期号
    /// </summary>
    public string PeriodicalNO { get; private set; }

    /// <summary>
    /// 开奖日期
    /// </summary>
    public DateTime DrawDate { get; private set; }

    /// <summary>
    /// 出球顺序（按摇出顺序排列的号码）
    /// </summary>
    public string OutBallOrder { get; private set; }

    /// <summary>
    /// 红球号码（逗号分隔，按大小顺序）
    /// </summary>
    public string RedBalls { get; private set; }

    /// <summary>
    /// 蓝球号码
    /// </summary>
    public string BlueBall { get; private set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedTime { get; private set; }

    /// <summary>
    /// 无参构造函数（供ORM使用）
    /// </summary>
    protected SSQHistory() { }

    /// <summary>
    /// 创建双色球历史记录（供EF Core使用）
    /// </summary>
    /// <param name="periodicalNO">开奖期号</param>
    /// <param name="drawDate">开奖日期</param>
    /// <param name="outBallOrder">出球顺序</param>
    /// <param name="redBalls">红球号码</param>
    /// <param name="blueBall">蓝球号码</param>
    public SSQHistory(string periodicalNO, DateTime drawDate, string outBallOrder, string redBalls, string blueBall)
    {
        PeriodicalNO = periodicalNO ?? throw new ArgumentNullException(nameof(periodicalNO));
        DrawDate = drawDate;
        OutBallOrder = outBallOrder ?? throw new ArgumentNullException(nameof(outBallOrder));
        RedBalls = redBalls ?? throw new ArgumentNullException(nameof(redBalls));
        BlueBall = blueBall ?? throw new ArgumentNullException(nameof(blueBall));
        CreatedTime = DateTime.Now;
    }

    /// <summary>
    /// 获取红球号码列表
    /// </summary>
    /// <returns>红球号码数组</returns>
    public string[] GetRedBallArray()
    {
        return RedBalls.Split(',', StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// 获取出球顺序列表
    /// </summary>
    /// <returns>出球顺序数组</returns>
    public string[] GetOutBallOrderArray()
    {
        return OutBallOrder.Split(',', StringSplitOptions.RemoveEmptyEntries);
    }
}