# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["TrafficPoliceService.csproj", "./"]
RUN dotnet restore "TrafficPoliceService.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "TrafficPoliceService.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "TrafficPoliceService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copy published app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "TrafficPoliceService.dll"]
