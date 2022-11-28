﻿using RabbitMQ.Client;
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

channel.BasicQos(0, 1, false);
var consumer = new EventingBasicConsumer(channel);

var queueName = channel.QueueDeclare().QueueName;

string routeKey = "*.Error.*";

//We use QueueBind so that when the consumer was closed then the queue will be gone.
channel.QueueBind(queueName, "logs-topic", routeKey);

channel.BasicConsume(queueName, false, consumer);

Console.WriteLine("Logs listening ...");

consumer.Received += (object sender, BasicDeliverEventArgs e) =>
{
    var message = Encoding.UTF8.GetString(e.Body.ToArray());

    Thread.Sleep(1500);
    Console.WriteLine("Message : " + message);

    //File.AppendAllText("log-critical.text", message + "\n\n");
    channel.BasicAck(e.DeliveryTag, false);
};

Console.ReadLine();
