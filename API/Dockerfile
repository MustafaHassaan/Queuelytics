# ========================
# 1. Base Runtime Image
# ========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# ========================
# 2. Build Image
# ========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# 👇 لو المشروع داخل مجلد اسمه API
COPY ["API/API.csproj", "API/"]
RUN dotnet restore "API/API.csproj"

# 👇 انسخ كل الكود
COPY . .

WORKDIR "/src/API"
RUN dotnet build "API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ========================
# 3. Publish Image
# ========================
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ========================
# 4. Final Runtime Image
# ========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# 👇 انسخ الملفات المنشورة من المرحلة السابقة
COPY --from=publish /app/publish .
COPY Shared/Data /app/Data
ENTRYPOINT ["dotnet", "API.dll"]
