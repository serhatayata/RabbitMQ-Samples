using RabbitMQ.Client;
using RabbitMQ.Client.Events;
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

channel.BasicQos(0, 1, false);
var consumer = new EventingBasicConsumer(channel);

var queueName = channel.QueueDeclare().QueueName;

Dictionary<string, object> headers = new();

headers.Add("format", "pdf");
//headers.Add("shape2", "a4");
headers.Add("shape", "a4");
headers.Add("x-match", "all"); //All of them has to match
//headers.Add("x-match", "any"); //Just one of them has to match

//We use QueueBind so that when the consumer was closed then the queue will be gone.
channel.QueueBind(queueName, "header-exchange", String.Empty, headers);

channel.BasicConsume(queueName, false, consumer);

Console.WriteLine("Logs listening ...");

consumer.Received += (object sender, BasicDeliverEventArgs e) =>
{
    var message = Encoding.UTF8.GetString(e.Body.ToArray());

    var product = JsonSerializer.Deserialize<Product>(message);

    Thread.Sleep(1500);
    Console.WriteLine($"Message : {product.Id} - {product.Name} - {product.Price} - {product.Stock}");


    channel.BasicAck(e.DeliveryTag, false);
};

Console.ReadLine();
