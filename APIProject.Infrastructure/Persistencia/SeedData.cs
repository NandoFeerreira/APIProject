using APIProject.Application.Interfaces;
using APIProject.Domain.Entidades;
using APIProject.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace APIProject.Infrastructure.Persistencia
{
    public class DatabaseInitializer
    {
        // Classe auxiliar para logging
    }

    public static class SeedData
    {
        public static async Task InicializarBancoDeDados(IHost app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<ApplicationDbContext>();
                    var hashService = services.GetRequiredService<IHashService>();
                    var usuarioRepositorio = services.GetRequiredService<IUsuarioRepositorio>();

                    await context.Database.EnsureCreatedAsync();

                    // Verificar se já existe um usuário administrador
                    if (!context.Usuarios.Any(u => u.Email == "admin@exemplo.com"))
                    {
                        // Criar usuário administrador
                        var senhaHash = hashService.CriarHash("Admin@123");
                        var admin = new Usuario("Administrador", "admin@exemplo.com", senhaHash);
                        
                        // Adicionar perfil de administrador
                        admin.Perfis.Add("Admin");
                        
                        await usuarioRepositorio.AdicionarAsync(admin);
                        await usuarioRepositorio.SalvarAsync();
                    }
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<DatabaseInitializer>>();
                    logger.LogError(ex, "Ocorreu um erro ao inicializar o banco de dados.");
                }
            }
        }
    }
}