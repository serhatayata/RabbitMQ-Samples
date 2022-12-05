using RabbitMQ.Client;
using Shared;
using System.Text;
using System.Text.Json;

var factory = new ConnectionFactory()
{
    Port = 5672
};

factory.UserName = "guest";
factory.Password = "guest";

using var connection = factory.CreateConnection();

var channel = connection.CreateModel();

channel.ExchangeDeclare(exchange: "header-exchange", durable: true, type: ExchangeType.Headers);

Dictionary<string, object> headers = new();

headers.Add("format", "pdf");
headers.Add("shape", "a4");

var properties = channel.CreateBasicProperties();
properties.Headers = headers;
properties.Persistent = true; //Messages will be persistent even if rabbitmq is restarted... This can be used with any other exchanges

var product = new Product() { Id = 1, Name = "Pencil", Price = 25, Stock = 100 };

var productJsonString = JsonSerializer.Serialize(product);

channel.BasicPublish("header-exchange", string.Empty, properties, Encoding.UTF8.GetBytes(productJsonString));

Console.ReadLine();