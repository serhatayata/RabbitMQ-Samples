using RabbitMQ.Client;
using System.Text;

var factory = new ConnectionFactory()
{
    Port=5672
};

factory.UserName = "guest";
factory.Password = "guest";

using var connection = factory.CreateConnection();

var channel = connection.CreateModel();

channel.ExchangeDeclare(exchange: "logs-fanout", durable: true, type: ExchangeType.Fanout);

Enumerable.Range(1, 50).ToList().ForEach(x =>
{
    string message = $"log {x}";

    var messageBody = Encoding.UTF8.GetBytes(message);

    channel.BasicPublish(exchange:"logs-fanout", routingKey: "" , basicProperties: null, body: messageBody);

    Console.WriteLine("Message sent!");
});

Console.ReadLine();
