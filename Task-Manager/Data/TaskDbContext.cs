using Microsoft.EntityFrameworkCore;
using Task_Manager.Extensions;
using Task_Manager.Models;

namespace Task_Manager.Data
{
    public class TaskDbContext(IConfiguration configuration) : DbContext
    {

        public DbSet<Chamado> Chamados { get; set; }

        public DbSet<Usuario> Usuarios { get; set; }

        public DbSet<Comentario> Comentarios { get; set; }

        public DbSet<AuditLog> AuditLogs { get; set; }

        public DbSet<SystemLog> SystemLogs { get; set; }

        public DbSet<Grupo> Grupos { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL(configuration.GetConnectionString("DefaultConnection")!);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ConfigureDatabase();
        }
    }
}