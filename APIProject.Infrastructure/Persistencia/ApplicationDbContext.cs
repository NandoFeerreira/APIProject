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
            
            modelBuilder.Entity<Usuario>()
                .HasMany(u => u.RefreshTokens)
                .WithOne(rt => rt.Usuario)
                .HasForeignKey(rt => rt.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RefreshToken>()
                .HasKey(rt => rt.Id);

            modelBuilder.Entity<TokenInvalidado>()
           .HasKey(t => t.Id);

            modelBuilder.Entity<TokenInvalidado>()
                .HasOne(t => t.Usuario)
                .WithMany()
                .HasForeignKey(t => t.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(modelBuilder);
        }
    }
}
