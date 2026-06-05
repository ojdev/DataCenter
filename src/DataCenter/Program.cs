var builder = WebApplication.CreateBuilder(args);

// 添加 HttpClient
builder.Services.AddHttpClient();

// 配置数据库连接 - 解析环境变量占位符
var connectionString = ResolveEnvironmentVariables(builder.Configuration.GetConnectionString("Default"));
builder.Services.AddDbContext<DataCenterDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorCodesToAdd: null);
    });
});

// 注册仓储和服务
builder.Services.AddScoped<ISSQHistoryRepository, SSQHistoryRepository>();
builder.Services.AddScoped<ISSQHttpCollectUtility, SSQHttpCollectUtility>();
builder.Services.AddScoped<ISSQCollectService, SSQCollectService>();

var app = builder.Build();

app.UseHttpsRedirection();

// 健康检查端点
app.MapGet("/healthz", async (DataCenterDbContext dbContext) =>
{
    try
    {
        // 检查数据库连接
        await dbContext.Database.CanConnectAsync();
        return Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow });
    }
    catch (Exception ex)
    {
        return Results.Json(new { Status = "Unhealthy", Error = ex.Message, Timestamp = DateTime.UtcNow }, statusCode: 503);
    }
})
.WithName("HealthCheck");

// 自动采集接口
app.MapPost("/api/ssq/auto-collect", async (ISSQCollectService collectService, ISSQHistoryRepository repository) =>
{
    try
    {
        var totalCount = await repository.CountAsync();

        int collectedCount;
        string message;

        if (totalCount == 0)
        {
            var latestPeriodNO = await collectService.GetLatestPeriodicalNOAsync();
            collectedCount = await collectService.CollectRangeAsync("03001", latestPeriodNO);
            message = $"首次采集完成，从03001期到{latestPeriodNO}期，共采集 {collectedCount} 条记录";
        }
        else
        {
            collectedCount = await collectService.CollectAndSaveLatestAsync();
            message = $"增量采集完成，共采集 {collectedCount} 条新记录";
        }

        return Results.Ok(new
        {
            Message = message,
            CollectedCount = collectedCount,
            TotalCount = await repository.CountAsync(),
            IsFirstTime = totalCount == 0
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
})
.WithName("AutoCollect");

app.Run();

/// <summary>
/// 解析连接字符串中的环境变量占位符
/// 支持格式: ${ENV_VAR:default_value}
/// </summary>
/// <param name="connectionString">原始连接字符串</param>
/// <returns>解析后的连接字符串</returns>
string ResolveEnvironmentVariables(string connectionString)
{
    if (string.IsNullOrEmpty(connectionString))
    {
        return connectionString;
    }

    // 匹配 ${VAR_NAME:default} 格式
    var pattern = @"\$\{([^}]+)\}";
    return System.Text.RegularExpressions.Regex.Replace(connectionString, pattern, match =>
    {
        var expr = match.Groups[1].Value;
        var parts = expr.Split(':');
        var varName = parts[0];
        var defaultValue = parts.Length > 1 ? parts[1] : string.Empty;

        // 从环境变量获取值，不存在则使用默认值
        var value = Environment.GetEnvironmentVariable(varName);
        return string.IsNullOrEmpty(value) ? defaultValue : value;
    });
}