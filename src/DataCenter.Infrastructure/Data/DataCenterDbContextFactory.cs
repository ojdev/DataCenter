namespace DataCenter.Infrastructure.Data;

/// <summary>
/// 设计时数据库上下文工厂（用于EF Core CLI工具）
/// </summary>
public class DataCenterDbContextFactory : IDesignTimeDbContextFactory<DataCenterDbContext>
{
    /// <summary>
    /// 创建数据库上下文实例
    /// </summary>
    /// <param name="args">命令行参数</param>
    /// <returns>数据库上下文实例</returns>
    public DataCenterDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DataCenterDbContext>();

        // 使用硬编码的连接字符串（仅用于设计时迁移）
        var connectionString = "Host=120.26.225.76;Port=5432;Database=DataCenterDB;Username=postgres;Password=+!@Xp^14g7uQbazj+W,Z;";

        optionsBuilder.UseNpgsql(connectionString);

        return new DataCenterDbContext(optionsBuilder.Options);
    }
}