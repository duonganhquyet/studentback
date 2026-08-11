# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY [".", "./"]
RUN dotnet restore
RUN dotnet publish -c Release -r linux-x64 --self-contained false -o /app/publish 

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# 1. Ép .NET chạy ở cổng 8080
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# 2. Tắt Globalization để tránh lỗi 139 (thiếu thư viện ICU của Linux)
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

# 3. FIX LỖI INOTIFY: Tắt tính năng tự động theo dõi file appsettings.json
ENV DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false

# 4. Sử dụng tên file DLL chuẩn xác lấy từ log của bạn
ENTRYPOINT ["dotnet", "StudentAcademicManagement.Api.dll"]