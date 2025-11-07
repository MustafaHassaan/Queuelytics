using Infrastructure.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProducerService.Analytics
{
    public class Workerproducer : BackgroundService
    {
        private readonly ILogger<Workerproducer> _logger;
        private readonly RabbitMqPublisher _publisher;
        private readonly IConfiguration _config;

        public Workerproducer(ILogger<Workerproducer> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;

            var host = _config["RabbitMQ:Host"];
            var username = _config["RabbitMQ:Username"];
            var password = _config["RabbitMQ:Password"];

            _publisher = new RabbitMqPublisher(host, username, password);
            _publisher.Initialize();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ProducerService started.");

            var gaJson = await File.ReadAllTextAsync(Path.Combine(Directory.GetCurrentDirectory(), "Data", "GAData.json"));
            var psiJson = await File.ReadAllTextAsync(Path.Combine(Directory.GetCurrentDirectory(), "Data", "PSIData.json"));
            var gaData = JsonSerializer.Deserialize<List<GARecord>>(gaJson);
            var psiData = JsonSerializer.Deserialize<List<PSIRecord>>(psiJson);

            var combined = from ga in gaData
                           join psi in psiData
                           on new { ga.Page, ga.Date } equals new { psi.Page, psi.Date }
                           select new CombinedAnalyticsRecord
                           {
                               Date = DateTime.Parse(ga.Date),
                               Page = ga.Page,
                               Users = ga.Users,
                               Sessions = ga.Sessions,
                               Views = ga.Views,
                               PerformanceScore = psi.PerformanceScore,
                               LCPms = psi.LCP_ms,
                               ReceivedAt = DateTime.UtcNow
                           };

            foreach (var record in combined)
            {
                _publisher.Publish(record);
                _logger.LogInformation($"📤 Published → {record.Page} on {record.Date}");
                await Task.Delay(500, stoppingToken);
            }

            _logger.LogInformation("✅ All messages published successfully.");
        }

        public override void Dispose()
        {
            _publisher.Dispose();
            base.Dispose();
        }
    }
}
