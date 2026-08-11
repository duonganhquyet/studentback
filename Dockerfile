# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY [".", "./"]
RUN dotnet restore
RUN dotnet publish -c Release -r linux-x64 --self-contained false -o /app/publish 

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

# Cài đặt thư viện ICU (giải quyết tận gốc lỗi 139 để SQL Server hoạt động được)
RUN apt-get update && apt-get install -y libicu-dev && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app/publish .

# Ép .NET chạy ở cổng 8080
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Tắt tính năng tự động theo dõi file appsettings.json (Fix lỗi Inotify ở dòng 10)
ENV DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false

ENTRYPOINT ["dotnet", "StudentAcademicManagement.Api.dll"]