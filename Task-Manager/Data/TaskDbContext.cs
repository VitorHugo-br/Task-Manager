using Microsoft.EntityFrameworkCore;
using Task_Manager.Models;

namespace Task_Manager.Data
{
    public class TaskDbContext : DbContext
    {
        public DbSet<MyTask> Tasks { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<ErrorLog> ErrorLogs { get; set; }

        override protected void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL("Host=localhost;Port=3306;Database=mysqlDB;Username=user_vitorhugo;Password=vi@@2022");
        }
    }
}
