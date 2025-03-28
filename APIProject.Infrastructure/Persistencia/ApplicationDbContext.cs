using APIProject.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace APIProject.Infrastructure.Persistencia
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }      

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Aplicar todas as configurações de entidade do assembly
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(modelBuilder);
        }
    }
}