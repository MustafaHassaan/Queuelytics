using RabbitMQ.Client;
using System;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Messaging
{
    public class RabbitMqPublisher : IDisposable
    {
        private readonly ConnectionFactory _factory;
        private IConnection? _connection;
        private IModel? _channel;
        private const string ExchangeName = "analytics.raw";
        private const string QueueName = "analytics.raw.q";

        public RabbitMqPublisher(string hostName, string userName, string password)
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

            // Declare exchange (fanout)
            _channel.ExchangeDeclare(
                exchange: ExchangeName,
                type: ExchangeType.Fanout,
                durable: true,
                autoDelete: false
            );

            // Declare queue (durable)
            _channel.QueueDeclare(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            // Bind queue to exchange
            _channel.QueueBind(
                queue: QueueName,
                exchange: ExchangeName,
                routingKey: string.Empty // fanout ignores routing key
            );

            Console.WriteLine($"[Publisher] ✅ Exchange & Queue declared: {ExchangeName} → {QueueName}");
        }

        public void Publish<T>(T message, int retryCount = 3)
        {
            if (_channel == null)
                throw new InvalidOperationException("Publisher not initialized. Call Initialize first.");

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            var props = _channel.CreateBasicProperties();
            props.Persistent = true; // <-- Make message persistent

            for (int attempt = 1; attempt <= retryCount; attempt++)
            {
                try
                {
                    _channel.BasicPublish(
                        exchange: ExchangeName,
                        routingKey: string.Empty,
                        basicProperties: props,
                        body: body
                    );

                    Console.WriteLine($"[Publisher] ✅ Sent → {typeof(T).Name}");
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Publisher] ⚠️ Error attempt {attempt}: {ex.Message}");

                    if (attempt == retryCount)
                        throw;

                    Thread.Sleep(500 * attempt); // simple exponential backoff
                }
            }
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
