using APIProject.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace APIProject.Infrastructure.Persistencia
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public  DbSet<Usuario> Usuarios { get; set; }
        public  DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<TokenInvalidado> TokensInvalidados { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Aplicar todas as configurações de entidades do assembly
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(modelBuilder);
        }
    }
}
