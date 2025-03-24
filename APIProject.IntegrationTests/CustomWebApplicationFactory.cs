using APIProject.API;
using APIProject.Application.Interfaces;
using APIProject.Domain.Entidades;
using APIProject.Infrastructure.Persistencia;
using Bogus;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace APIProject.IntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly string _databaseName;

        public CustomWebApplicationFactory(string databaseName)
        {
            _databaseName = databaseName;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            // Configurar o arquivo de configuração de teste
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.Testing.json"), optional: false);
            });

            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
            });

            builder.ConfigureServices(services =>
            {
                // Remover o registro do DbContext existente
                var descriptors = services.Where(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                         d.ServiceType == typeof(DbContextOptions) ||
                         d.ServiceType == typeof(ApplicationDbContext));

                foreach (var descriptor in descriptors.ToList())
                {
                    services.Remove(descriptor);
                }

                // Adicionar o DbContext com banco de dados em memória
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase(_databaseName)
                           .EnableDetailedErrors()
                           .EnableSensitiveDataLogging());

                // Inicializar o banco de dados
                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<ApplicationDbContext>();
                    var logger = scopedServices.GetRequiredService<ILogger<CustomWebApplicationFactory>>();

                    try
                    {
                        db.Database.EnsureCreated();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Erro ao criar banco de dados para testes");
                        throw;
                    }
                }
            });
        }

        public void SeedTestData()
        {
            try
            {
                using var scope = Services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var hashService = scope.ServiceProvider.GetRequiredService<IHashService>();

                // Limpar todos os usuários existentes
                context.Usuarios.RemoveRange(context.Usuarios);
                context.SaveChanges();

                // Adicionar usuário específico para o teste de login
                var usuarioTeste = new Usuario("Usuário Teste", "teste@teste.com", hashService.CriarHash("senha123"));
                usuarioTeste.Ativo = true; // Garantir que está ativo
                context.Usuarios.Add(usuarioTeste);

                // Adicionar usuário de exemplo usado no teste
                var usuarioExemplo = new Usuario("Test Example", "test@example.com", hashService.CriarHash("Senha123!"));
                usuarioExemplo.Ativo = true; // Garantir que está ativo
                context.Usuarios.Add(usuarioExemplo);
               
                var usuarios = new List<Usuario>
                {
                    new Usuario("Usuário Adicional 1", "usuario1@teste.com", hashService.CriarHash("Senha123!")),
                    new Usuario("Usuário Adicional 2", "usuario2@teste.com", hashService.CriarHash("Senha123!")),
                    new Usuario("Usuário Adicional 3", "usuario3@teste.com", hashService.CriarHash("Senha123!"))
                };

                // Garantir que todos estão ativos
                foreach (var usuario in usuarios)
                {
                    usuario.Ativo = true;
                }

                context.Usuarios.AddRange(usuarios);
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao inicializar dados de teste: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw;
            }
        }
    }
}
