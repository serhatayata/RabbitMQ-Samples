using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RabbitMQProjects.ExcelCreateApp.Models;

namespace RabbitMQProjects.ExcelCreateApp.Data
{
    public class AppDbContext:IdentityDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options):base(options)
        {


        }

        public DbSet<UserFile> UserFiles { get; set; }

    }
}
