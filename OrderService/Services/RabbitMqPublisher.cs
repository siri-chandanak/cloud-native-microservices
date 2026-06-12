using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace OrderService.Services;

public class RabbitMqPublisher
{
    public void Publish<T>(string queueName, T message)
    {
        var factory = new ConnectionFactory()
        {
            HostName = "localhost"
        };

        using var connection = factory.CreateConnection();

        using var channel = connection.CreateModel();

        channel.QueueDeclare(
            queue: queueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var body = Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(message));

        channel.BasicPublish(
            exchange: "",
            routingKey: queueName,
            basicProperties: null,
            body: body);
    }
}