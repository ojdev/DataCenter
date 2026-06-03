namespace DataCenter.Infrastructure.Data;

/// <summary>
/// 数据中心数据库上下文
/// </summary>
public class DataCenterDbContext : DbContext
{
    /// <summary>
    /// 双色球历史数据集
    /// </summary>
    public DbSet<SSQHistory> SSQHistories { get; set; }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="options">数据库上下文选项</param>
    public DataCenterDbContext(DbContextOptions<DataCenterDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// 配置实体映射
    /// </summary>
    /// <param name="modelBuilder">模型构建器</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 应用实体配置
        modelBuilder.ApplyConfiguration(new SSQHistoryConfiguration());
    }
}