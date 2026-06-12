using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Events;

namespace NotificationService.Consumers;

public class OrderCreatedConsumer : BackgroundService
{
    protected override Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory()
        {
            HostName = "localhost"
        };

        var connection =
            factory.CreateConnection();

        var channel =
            connection.CreateModel();

        channel.QueueDeclare(
            queue: "order-created",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var consumer =
            new EventingBasicConsumer(channel);

        consumer.Received +=
            (model, ea) =>
            {
                var body = ea.Body.ToArray();

                var json =
                    Encoding.UTF8.GetString(body);

                var order =
                    JsonSerializer.Deserialize<OrderCreatedEvent>(json);

                Console.WriteLine(
                    $"Email sent for order {order?.OrderId}");
            };

        channel.BasicConsume(
            queue: "order-created",
            autoAck: true,
            consumer: consumer);

        return Task.CompletedTask;
    }
}