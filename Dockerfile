# Base ASP.NET 8.0 Runtime Image for running the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# .NET 8.0 SDK Image for building and restoring the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy all project files to optimize Docker cache layer restoration
COPY ["DotNet8.EasyTripBackend/DotNet8.EasyTripBackend.csproj", "DotNet8.EasyTripBackend/"]
COPY ["DotNet8.EasyTripBackendApi.DbService/DotNet8.EasyTripBackendApi.DbService.csproj", "DotNet8.EasyTripBackendApi.DbService/"]
COPY ["DotNet8.EasyTripBackendApi.Models/DotNet8.EasyTripBackendApi.Models.csproj", "DotNet8.EasyTripBackendApi.Models/"]
COPY ["DotNet8.EasyTripBackendApi.Shared/DotNet8.EasyTripBackendApi.Shared.csproj", "DotNet8.EasyTripBackendApi.Shared/"]

# Restore dependencies
RUN dotnet restore "DotNet8.EasyTripBackend/DotNet8.EasyTripBackend.csproj"

# Copy the rest of the source code
COPY . .

# Build and Publish the main Web API project
WORKDIR "/src/DotNet8.EasyTripBackend"
RUN dotnet publish "DotNet8.EasyTripBackend.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final Production Image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DotNet8.EasyTripBackend.dll"]
