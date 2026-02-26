using Microsoft.EntityFrameworkCore;
using Task_Manager.Models;

namespace Task_Manager.Data
{
    public class TaskDbContext(IConfiguration configuration) : DbContext
    {

        private readonly IConfiguration _configuration = configuration;

        public DbSet<MyTask> Tasks { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<ErrorLog> ErrorLogs { get; set; }

        public DbSet<Comment> Comments { get; set; }

        override protected void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL(_configuration.GetConnectionString("DefaultConnection")!);
        }
    }
}
