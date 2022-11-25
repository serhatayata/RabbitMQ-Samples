using RabbitMQ.Client;
using RabbitMQProject.Publisher;
using System.Text;

var factory = new ConnectionFactory()
{
    Port = 5672
};

factory.UserName = "guest";
factory.Password = "guest";

using var connection = factory.CreateConnection();

var channel = connection.CreateModel();

channel.ExchangeDeclare(exchange: "logs-direct", durable: true, type: ExchangeType.Direct);

Enum.GetNames(typeof(LogNames)).ToList().ForEach(x =>
{
    var routeKey = $"route-{x}";

    var queueName = $"direct-queue-{x}";
    channel.QueueDeclare(queueName, true, false, false);

    channel.QueueBind(queueName, "logs-direct", routeKey,null);
});

Enumerable.Range(1, 50).ToList().ForEach(x =>
{
    LogNames log = (LogNames)new Random().Next(1, 5);

    string message = $"log-type: {log}";

    var messageBody = Encoding.UTF8.GetBytes(message);

    var routeKey = $"route-{log}";

    channel.BasicPublish(exchange: "logs-direct", routingKey: routeKey, basicProperties: null, body: messageBody);

    Console.WriteLine($"Message sent : {message}");
});

Console.ReadLine();
