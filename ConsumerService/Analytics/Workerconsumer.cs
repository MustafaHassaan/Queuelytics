using Application.Interfaces;
using Domain.Models;
using Infrastructure.Dataaccess;
using Infrastructure.Messaging;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsumerService.Analytics
{
    public class Workerconsumer : BackgroundService
    {
        private readonly ILogger<Workerconsumer> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IUnitOfWork _IUW;

        private readonly RabbitMqConsumer _consumer;
        public Workerconsumer(ILogger<Workerconsumer> logger, IServiceScopeFactory scopeFactory, IConfiguration config)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;

            var host = config["RabbitMQ:Host"];
            var username = config["RabbitMQ:Username"];
            var password = config["RabbitMQ:Password"];
            _consumer = new RabbitMqConsumer(host, username, password);
            _consumer.Initialize();
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ConsumerService started.");

            _consumer.Consume(async (CombinedAnalyticsRecord record) =>
            {
                using var scope = _scopeFactory.CreateScope();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                for (int attempt = 1; attempt <= 3; attempt++)
                {
                    try
                    {
                        var RD = new RawData
                        {
                            Date = DateOnly.FromDateTime(record.Date),
                            Pages = record.Page,
                            Users = record.Users,
                            Sessions = record.Sessions,
                            Views = record.Views,
                            PerformanceScore = record.PerformanceScore,
                            LCPms = record.LCPms,
                            ReceivedAt = record.ReceivedAt
                        };
                        await uow.RawDatas.AddAsync(RD);
                        // 2️⃣ تحديث أو إنشاء DailyStat
                        var date = DateOnly.FromDateTime(record.Date);
                        var stat = await uow.DailyStats.GetByDateAsync(date);
                        if (stat == null)
                        {
                            stat = new DailyStat
                            {
                                Date = date,
                                TotalUsers = record.Users,
                                TotalSessions = record.Sessions,
                                TotalViews = record.Views,
                                AvgPerformance = record.PerformanceScore,
                                LastUpdatedAt = DateTime.UtcNow
                            };
                            await uow.DailyStats.AddAsync(stat);
                        }
                        else
                        {
                            stat.TotalUsers += record.Users;
                            stat.TotalSessions += record.Sessions;
                            stat.TotalViews += record.Views;
                            stat.AvgPerformance = (stat.AvgPerformance + record.PerformanceScore) / 2;
                            stat.LastUpdatedAt = DateTime.UtcNow;
                        }
                        await uow.CompleteAsync();
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"❌ Save failed (attempt {attempt}): {ex.Message}");
                        await Task.Delay(500 * attempt);
                    }
                }
            });

            await Task.CompletedTask;
        }
        public override void Dispose()
        {
            _consumer.Dispose();
            base.Dispose();
        }
    }
}
