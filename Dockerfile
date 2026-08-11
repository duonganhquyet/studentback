# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY [".", "./"]
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0-bookworm-slim AS final
WORKDIR /app
COPY --from=build /app/publish .

# Ép .NET chạy ở cổng 8080 để tương thích với Render
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "StudentAcademicManagement.Api.dll"]