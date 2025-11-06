using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Messaging
{
    public class RabbitMqConsumer : IDisposable
    {
        private readonly ConnectionFactory _factory;
        private IConnection? _connection;
        private IModel? _channel;
        private const string QueueName = "analytics.raw.q";

        public RabbitMqConsumer(string hostName, string userName, string password)
        {
            _factory = new ConnectionFactory
            {
                HostName = hostName,
                UserName = userName,
                Password = password,
                Port = 5672,
                DispatchConsumersAsync = true
            };
        }

        public void Initialize()
        {
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            Console.WriteLine($"[Consumer] ✅ Queue declared: {QueueName}");
        }

        public void Consume(Func<CombinedAnalyticsRecord, Task> handleMessage)
        {
            if (_channel == null)
                throw new InvalidOperationException("Consumer not initialized.");

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);

                try
                {
                    var record = JsonSerializer.Deserialize<CombinedAnalyticsRecord>(json);
                    if (record != null)
                    {
                        await handleMessage(record);
                        _channel.BasicAck(ea.DeliveryTag, multiple: false);
                        Console.WriteLine($"[Consumer] ✅ Ack → {record.Page} on {record.Date}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Consumer] ❌ Error: {ex.Message}");
                    // (Bonus) Send to DLQ if needed
                }
            };

            _channel.BasicConsume(
                queue: QueueName,
                autoAck: false,
                consumer: consumer
            );
        }

        public void Dispose()
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
        }
    }

}
