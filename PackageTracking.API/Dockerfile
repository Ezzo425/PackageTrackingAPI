# Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["PackageTracking.API/PackageTracking.API.csproj", "PackageTracking.API/"]
RUN dotnet restore "PackageTracking.API/PackageTracking.API.csproj"

COPY . .
WORKDIR "/src/PackageTracking.API"
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "PackageTracking.API.dll"]