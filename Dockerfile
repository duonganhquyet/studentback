# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files for cache optimization
COPY ["StudentAcademicManagement.Api/StudentAcademicManagement.Api.csproj", "StudentAcademicManagement.Api/"]
COPY ["StudentAcademicManagement.Application/StudentAcademicManagement.Application.csproj", "StudentAcademicManagement.Application/"]
COPY ["StudentAcademicManagement.Domain/StudentAcademicManagement.Domain.csproj", "StudentAcademicManagement.Domain/"]
COPY ["StudentAcademicManagement.Infrastructure/StudentAcademicManagement.Infrastructure.csproj", "StudentAcademicManagement.Infrastructure/"]

RUN dotnet restore "StudentAcademicManagement.Api/StudentAcademicManagement.Api.csproj"

# Copy full source code and build
COPY . .
WORKDIR "/src/StudentAcademicManagement.Api"
RUN dotnet publish "StudentAcademicManagement.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV PORT=8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "StudentAcademicManagement.Api.dll"]
