namespace DataCenter.Infrastructure.EntityConfigurations;

/// <summary>
/// 双色球历史数据实体配置
/// </summary>
public class SSQHistoryConfiguration : IEntityTypeConfiguration<SSQHistory>
{
    /// <summary>
    /// 配置实体映射
    /// </summary>
    /// <param name="builder">实体类型构建器</param>
    public void Configure(EntityTypeBuilder<SSQHistory> builder)
    {
        // 设置表名
        builder.ToTable("SSQHistories");

        // 设置主键
        builder.HasKey(e => e.Id);

        // 配置属性
        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .HasComment("主键ID");

        builder.Property(e => e.PeriodicalNO)
            .IsRequired()
            .HasMaxLength(20)
            .HasComment("开奖期号");

        builder.Property(e => e.DrawDate)
            .IsRequired()
            .HasColumnType("timestamp without time zone")
            .HasComment("开奖日期");

        builder.Property(e => e.OutBallOrder)
            .IsRequired()
            .HasMaxLength(50)
            .HasComment("出球顺序（按摇出顺序排列的号码）");

        builder.Property(e => e.RedBalls)
            .IsRequired()
            .HasMaxLength(50)
            .HasComment("红球号码（逗号分隔，按大小顺序）");

        builder.Property(e => e.BlueBall)
            .IsRequired()
            .HasMaxLength(10)
            .HasComment("蓝球号码");

        builder.Property(e => e.CreatedTime)
            .IsRequired()
            .HasColumnType("timestamp without time zone")
            .HasComment("创建时间");

        // 创建唯一索引
        builder.HasIndex(e => e.PeriodicalNO)
            .IsUnique()
            .HasDatabaseName("IX_SSQHistories_PeriodicalNO");

        // 创建普通索引
        builder.HasIndex(e => e.DrawDate)
            .HasDatabaseName("IX_SSQHistories_DrawDate");

        builder.HasIndex(e => e.CreatedTime)
            .HasDatabaseName("IX_SSQHistories_CreatedTime");
    }
}