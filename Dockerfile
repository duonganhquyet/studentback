# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY [".", "./"]
RUN dotnet restore
# Ép build chuẩn cho môi trường Linux của Render
RUN dotnet publish -c Release -r linux-x64 --self-contained false -o /app/publish 

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Ép .NET chạy ở cổng 8080
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Tắt Globalization để tránh lỗi 139 (thiếu thư viện ICU)
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

ENTRYPOINT ["dotnet", "StudentAcademicManagement.Api.dll"]