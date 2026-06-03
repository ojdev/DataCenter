var builder = WebApplication.CreateBuilder(args);

// 添加 HttpClient
builder.Services.AddHttpClient();

// 配置数据库连接
var connectionString = builder.Configuration.GetConnectionString("Default");
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
        return Results.StatusCode(503, new { Status = "Unhealthy", Error = ex.Message, Timestamp = DateTime.UtcNow });
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