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

channel.ExchangeDeclare(exchange: "logs-topic", durable: true, type: ExchangeType.Topic);

Random rnd = new Random();
Enumerable.Range(1, 50).ToList().ForEach(x =>
{
    LogNames log1 = (LogNames)rnd.Next(1, 5);
    LogNames log2 = (LogNames)rnd.Next(1, 5);
    LogNames log3 = (LogNames)rnd.Next(1, 5);

    

    var routeKey = $"{log1}.{log2}.{log3}";

    string message = $"log-type: {log1}-{log2}-{log3}";
    var messageBody = Encoding.UTF8.GetBytes(message);

    channel.BasicPublish(exchange: "logs-topic", routingKey: routeKey, basicProperties: null, body: messageBody);

    Console.WriteLine($"Message sent : {message}");
});

Console.ReadLine();