using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RabbitMQProjects.ExcelCreateApp.Data;
using RabbitMQProjects.ExcelCreateApp.Hubs;

namespace RabbitMQProjects.ExcelCreateApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<MyHub> _hubContext;

        public FilesController(AppDbContext context, IHubContext<MyHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file, int fileId)
        {
            if (file is not { Length: > 0 })
                return BadRequest();

            var userFile = await _context.UserFiles.FirstOrDefaultAsync(f => f.Id == fileId);

            var filePath = userFile.FileName + Path.GetExtension(file.FileName);
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/files", filePath);
            using FileStream stream = new(path, FileMode.Create);

            await file.CopyToAsync(stream);
            userFile.CreatedDate = DateTime.Now;
            userFile.FilePath = filePath;
            userFile.FileStatus = Models.FileStatus.Completed;
            await _context.SaveChangesAsync();

            //SignalR notification
            await _hubContext.Clients.User(userFile.UserId).SendAsync("CompletedFile");

            return Ok();
        }

    }
}
