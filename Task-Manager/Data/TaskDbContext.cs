using Microsoft.EntityFrameworkCore;
using Task_Manager.Models;

namespace Task_Manager.Data
{
    public class TaskDbContext(IConfiguration configuration) : DbContext
    {

        public DbSet<MyTask> Tasks { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<Comment> Comments { get; set; }
        
        public DbSet<AuditLog> AuditLogs { get; set; }
        
        public DbSet<SystemLog> SystemLogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL(configuration.GetConnectionString("DefaultConnection")!);
        }
    }
}