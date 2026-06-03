# 基于 .NET 10 SDK 镜像进行构建
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 复制解决方案和项目文件
COPY ["src/DataCenter.Domain/DataCenter.Domain.csproj", "src/DataCenter.Domain/"]
COPY ["src/DataCenter.Application/DataCenter.Application.csproj", "src/DataCenter.Application/"]
COPY ["src/DataCenter.Infrastructure/DataCenter.Infrastructure.csproj", "src/DataCenter.Infrastructure/"]
COPY ["src/DataCenter/DataCenter.csproj", "src/DataCenter/"]

# 恢复依赖
RUN dotnet restore "src/DataCenter/DataCenter.csproj"

# 复制所有源代码
COPY . .

# 构建并发布应用
WORKDIR "/src/src/DataCenter"
RUN dotnet publish "DataCenter.csproj" -c Release -o /app/publish

# 基于 .NET 10 运行时镜像
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# 设置环境变量
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# 复制发布内容
COPY --from=build /app/publish .

# 暴露端口
EXPOSE 8080

# 启动应用
ENTRYPOINT ["dotnet", "DataCenter.dll"]
