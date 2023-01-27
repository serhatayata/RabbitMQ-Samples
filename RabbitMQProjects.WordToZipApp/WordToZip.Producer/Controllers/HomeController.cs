using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Diagnostics;
using System.Text;
using WordToZip.Producer.Models;

namespace WordToZip.Producer.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ConnectionFactory _connectionFactory;

        public HomeController(ConnectionFactory connectionFactory, ILogger<HomeController> logger)
        {
            _logger = logger;
            _connectionFactory = connectionFactory;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult WordToZipPage()
        {
            return View();
        }

        [HttpPost]
        public IActionResult WordToZipPage(Models.WordToZip model)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.ExchangeDeclare("convert-zip-exchange", ExchangeType.Direct, true, false, null);
            channel.QueueDeclare(queue: "File", durable: true, exclusive: false, autoDelete: false, arguments: null);

            channel.QueueBind("File", "convert-zip-exchange", "WordToZip");

            MessageWordToZip messageWordToZip = new();

            using MemoryStream ms = new();
            //Copied to memory
            model.WordFile.CopyTo(ms);
            messageWordToZip.WordByte = ms.ToArray();
            messageWordToZip.Email = model.Email;
            messageWordToZip.FileName = Path.GetFileNameWithoutExtension(model.WordFile.FileName);

            string serializeMessage = System.Text.Json.JsonSerializer.Serialize(messageWordToZip);
            byte[] byteMessage = Encoding.UTF8.GetBytes(serializeMessage);
            var property = channel.CreateBasicProperties();
            property.Persistent = true;

            channel.BasicPublish("convert-zip-exchange", routingKey: "WordToZip", basicProperties: property, byteMessage);

            ViewBag.result = "Word dosyanız Zip dosyasına dönüştürüldükten sonra size e-mail olarak gönderilecektir";
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}