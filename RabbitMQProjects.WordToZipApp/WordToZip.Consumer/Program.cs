using ICSharpCode.SharpZipLib.Zip;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Spire.Doc;
using System.Text;
using System.Text.Json;
using WordToZip.Consumer;

var factory = new ConnectionFactory()
{
    Port = 5672,
    DispatchConsumersAsync = true
};

using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.ExchangeDeclare("convert-zip-exchange", ExchangeType.Direct, true, false, null);
channel.QueueBind(queue: "File", exchange: "convert-zip-exchange", "WordToZip");

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
        MessageWordToZip messageWordToPdf = JsonSerializer.Deserialize<MessageWordToZip>(deserializedString);
        doc.LoadFromStream(new MemoryStream(messageWordToPdf.WordByte), FileFormat.Docx2013);

        var fileName = $"word-forzip-{Guid.NewGuid().ToString().Substring(1, 10)}.docx";
        var filePath = fileName + Path.GetExtension(messageWordToPdf.FileName);

        var zipFileName = $"zipfile-{Guid.NewGuid().ToString().Substring(1, 10)}.zip";
        var zipFilePath = zipFileName + Path.GetExtension(messageWordToPdf.FileName);

        var path = Path.Combine(Directory.GetCurrentDirectory(), "Files", filePath);
        var zipPath = Path.Combine(Directory.GetCurrentDirectory(), "Files", zipFilePath);

        using FileStream stream = new(path, FileMode.Create);
        doc.SaveToStream(stream, FileFormat.Docx2013);

        await stream.CopyToAsync(stream);

        stream.Close();
        stream.Dispose();

        using ZipOutputStream zipOutputStream= new(System.IO.File.Create(zipFilePath));
        zipOutputStream.SetLevel(9);

        byte[] buffer = new byte[4096];

        ZipEntry entry = new(Path.GetFileName(filePath));
        entry.DateTime = DateTime.Now;
        entry.IsUnicodeText = true;
        zipOutputStream.PutNextEntry(entry);

        using FileStream streamForZip = File.OpenRead(fileName);
        int sourceBytes = streamForZip.Read(buffer, 0, buffer.Length);
        zipOutputStream.Write(buffer, 0, sourceBytes);
        zipOutputStream.Finish();
        zipOutputStream.Flush();
        zipOutputStream.Close();

        Console.WriteLine("Message was processed successfully");
        channel.BasicAck(e.DeliveryTag, false);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An erorr occured : {ex.InnerException.Message}");
    }
}

Console.WriteLine("Click to exit");
Console.ReadLine();