using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

var factory = new ConnectionFactory()
{
    Port = 5672
};

factory.UserName = "guest";
factory.Password = "guest";

using var connection = factory.CreateConnection();

var channel = connection.CreateModel();

var randomQueueName = channel.QueueDeclare().QueueName; //Random queue name (We do not create a queue instance, just a random name)
//channel.QueueDeclare(randomQueueName, true, false, false); //If we used this, when we close the consumer, the queue wouldn't be gone.

channel.QueueBind(randomQueueName, "logs-fanout", "", null);

channel.BasicQos(0, 1, false);
var consumer = new EventingBasicConsumer(channel);

channel.BasicConsume(randomQueueName, false, consumer);
Console.WriteLine("Logs listening ...");

consumer.Received += (object sender, BasicDeliverEventArgs e) =>
{
    var message = Encoding.UTF8.GetString(e.Body.ToArray());

    Thread.Sleep(1500);
    Console.WriteLine("Message : " + message);

    channel.BasicAck(e.DeliveryTag, false);
};

Console.ReadLine();
