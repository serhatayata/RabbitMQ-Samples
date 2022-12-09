using Microsoft.EntityFrameworkCore;
using RabbitMQProjects.WatermarkApp.Models;

namespace RabbitMQProjects.WatermarkApp.Data
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options):base(options)
        {

        }

        public DbSet<Product> Products { get; set; }
    }
}
