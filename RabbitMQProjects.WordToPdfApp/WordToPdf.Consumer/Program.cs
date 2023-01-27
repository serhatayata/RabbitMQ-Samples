using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Spire.Doc;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using WordToPdf.Consumer;

var factory = new ConnectionFactory()
{
    Port = 5672,
    DispatchConsumersAsync = true
};

using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.ExchangeDeclare("convert-exchange", ExchangeType.Direct, true, false, null);
channel.QueueBind(queue: "File", exchange:"convert-exchange", "WordToPdf");

channel.BasicQos(0, 1, false);
var consumer = new AsyncEventingBasicConsumer(channel);

channel.BasicConsume("File", false, consumer);
bool result = false;
consumer.Received += Consumer_Received;

async Task Consumer_Received(object? sender, BasicDeliverEventArgs e)
{
    try
    {
        Console.WriteLine("A message was received from the queue and processing");
        Document doc = new();
        var deserializedString = Encoding.UTF8.GetString(e.Body.ToArray());
        MessageWordToPdf messageWordToPdf = JsonSerializer.Deserialize<MessageWordToPdf>(deserializedString);
        doc.LoadFromStream(new MemoryStream(messageWordToPdf.WordByte), FileFormat.Docx2013);

        using MemoryStream ms = new();
        doc.SaveToStream(ms, FileFormat.PDF);

        result = EmailSend(email: messageWordToPdf.Email, memoryStream: ms, fileName: messageWordToPdf.FileName);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An erorr occured : {ex.InnerException.Message}");
    }

    if (result)
    {
        Console.WriteLine("Message was processed successfully");
        channel.BasicAck(e.DeliveryTag, false);
    }
}

Console.WriteLine("Click to exit");
Console.ReadLine();

static bool EmailSend(string email, MemoryStream memoryStream, string fileName)
{
	try
	{
        //It is needed to get the file not as 0kB
        memoryStream.Position = 0;

        System.Net.Mime.ContentType ct = new(System.Net.Mime.MediaTypeNames.Application.Pdf);

        Attachment attach = new(memoryStream, ct);
        attach.ContentDisposition.FileName = $"{fileName}.pdf";

        MailMessage mailMessage = new();
        SmtpClient smtpClient = new();

        mailMessage.From = new MailAddress("blabla@bla.com");
        mailMessage.To.Add(email);
        mailMessage.Subject = "Pdf File Creation | blabla.com";
        mailMessage.Body = "Word to Pdf convertion completed";
        mailMessage.IsBodyHtml = true;
        mailMessage.Attachments.Add(attach);

        smtpClient.Host = "mail.blabla.net";
        smtpClient.Port = 587;
        smtpClient.Credentials = new System.Net.NetworkCredential("admin@blabla.net", "passs1");
        smtpClient.Send(mailMessage);
        Console.WriteLine($"Result : PDF was sent to the {email}");
        memoryStream.Close();
        memoryStream.Dispose();
        return true;
    }
	catch (Exception ex)
	{
        //LOGGING
        Console.WriteLine($"A problem was occured : {ex.InnerException.Message}");
        return false;
	}
}