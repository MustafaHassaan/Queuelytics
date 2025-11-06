#!/bin/bash
set -e

echo "🚀 Waiting for SQL Server..."
until /opt/mssql-tools/bin/sqlcmd -S sqlserver -U sa -P "" -Q "SELECT 1" > /dev/null 2>&1; do
    echo "⏳ SQL Server is starting..."
    sleep 3
done

echo "🚀 Waiting for RabbitMQ..."
until nc -z rabbitmq 5672; do
    echo "⏳ RabbitMQ is starting..."
    sleep 3
done

echo "✅ All services are up. Starting API..."
dotnet API.dll
