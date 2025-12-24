# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["VehicleService.csproj", "./"]
RUN dotnet restore "VehicleService.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "VehicleService.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "VehicleService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copy published app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "VehicleService.dll"]
