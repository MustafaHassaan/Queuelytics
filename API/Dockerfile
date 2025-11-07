# ========================
# Build & Publish Stage
# ========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy ALL project files (including ConsumerService & ProducerService)
COPY ["API/API.csproj", "API/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
COPY ["Shared/Shared.csproj", "Shared/"]
COPY ["ConsumerService/ConsumerService.csproj", "ConsumerService/"]
COPY ["ProducerService/ProducerService.csproj", "ProducerService/"]

# Restore
RUN dotnet restore "API/API.csproj"

# Copy ALL source code
COPY API/ API/
COPY Application/ Application/
COPY Domain/ Domain/
COPY Infrastructure/ Infrastructure/
COPY Shared/ Shared/
COPY ConsumerService/ ConsumerService/
COPY ProducerService/ ProducerService/

# Publish API
WORKDIR "/src/API"
RUN dotnet publish "API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ========================
# Runtime Stage
# ========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "API.dll"]