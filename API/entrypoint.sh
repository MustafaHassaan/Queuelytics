#!/bin/bash
set -e

echo "🔄 Waiting for SQL Server to be ready..."

# انتظر SQL Server
max_attempts=30
attempt=0
until /opt/mssql-tools18/bin/sqlcmd -S sqlserver -U sa -P "${SA_PASSWORD}" -Q "SELECT 1" -C > /dev/null 2>&1
do
  attempt=$((attempt + 1))
  if [ $attempt -eq $max_attempts ]; then
    echo "❌ SQL Server did not become ready in time"
    exit 1
  fi
  echo "⏳ SQL Server is unavailable - sleeping (attempt $attempt/$max_attempts)"
  sleep 3
done

echo "✅ SQL Server is up"

# تحقق من وجود الملف
if [ ! -f "API.dll" ]; then
  echo "❌ API.dll not found!"
  ls -la
  exit 1
fi

echo "🔄 Running migrations..."
dotnet API.dll --migrate || echo "⚠️ Migration failed or already applied"

echo "🚀 Starting application..."
exec dotnet API.dll