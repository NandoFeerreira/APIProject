using APIProject.API;
using APIProject.Application.Interfaces;
using APIProject.Domain.Entidades;
using APIProject.Domain.Interfaces;
using APIProject.Infrastructure.Persistencia;
using APIProject.Infrastructure.Persistencia.Repositorios;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace APIProject.IntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly string _databaseName;
        
        public CustomWebApplicationFactory()
        {
            _databaseName = $"TestDb-{Guid.NewGuid()}";
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
                logging.AddDebug();
            });

            builder.ConfigureServices(services =>
            {
                // Remover os registros do DbContext existentes
                RemoveDbContextRegistration(services);

                // Adicionar banco em memória para testes
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_databaseName);
                    options.EnableDetailedErrors();
                    options.EnableSensitiveDataLogging();
                });

                // Garantir que os serviços necessários para testes estejam registrados
                services.AddScoped<IUnitOfWork, UnitOfWork>();
            });
        }

        // Helper para remover registros de DbContext existentes
        private void RemoveDbContextRegistration(IServiceCollection services)
        {
            var descriptors = services.Where(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                     d.ServiceType == typeof(DbContextOptions) ||
                     d.ServiceType == typeof(ApplicationDbContext)).ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }
        }

        public void SeedTestData()
        {
            try
            {
                using var scope = Services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var hashService = scope.ServiceProvider.GetRequiredService<IHashService>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<CustomWebApplicationFactory>>();

                // Limpar dados existentes
                context.Usuarios.RemoveRange(context.Usuarios);
                context.RefreshTokens.RemoveRange(context.RefreshTokens);
                context.TokensInvalidados.RemoveRange(context.TokensInvalidados);
                context.SaveChanges();
                
                logger.LogInformation("Banco de dados limpo para inicialização de teste");

                // Adicionar usuário de teste
                var senhaCriptografada = hashService.CriarHash("senha123");
                var usuarioTeste = new Usuario("Usuário Teste", "teste@teste.com", senhaCriptografada);
                usuarioTeste.Ativo = true;
                
                context.Usuarios.Add(usuarioTeste);
                context.SaveChanges();
                
                logger.LogInformation("Usuário de teste criado: {Email}, {Nome}, {Ativo}", 
                    usuarioTeste.Email, usuarioTeste.Nome, usuarioTeste.Ativo);

                // Verificar se o usuário foi adicionado
                var usuarioVerificacao = context.Usuarios.FirstOrDefault(u => u.Email == "teste@teste.com");
                if (usuarioVerificacao != null)
                {
                    logger.LogInformation("Usuário verificado: {Email}, {Nome}, {Ativo}, {Id}",
                        usuarioVerificacao.Email, usuarioVerificacao.Nome, usuarioVerificacao.Ativo, usuarioVerificacao.Id);
                }
                else
                {
                    logger.LogError("Usuário não encontrado após adição");
                    throw new Exception("Falha ao adicionar usuário de teste");
                }
            }
            catch (Exception ex)
            {
                var logger = Services.GetRequiredService<ILogger<CustomWebApplicationFactory>>();
                logger.LogError(ex, "Erro ao inicializar dados de teste");
                throw;
            }
        }

        public void ResetDatabase()
        {
            using var scope = Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var hashService = scope.ServiceProvider.GetRequiredService<IHashService>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<CustomWebApplicationFactory>>();

            try
            {
                // Limpar todos os dados
                context.Usuarios.RemoveRange(context.Usuarios);
                context.RefreshTokens.RemoveRange(context.RefreshTokens);
                context.TokensInvalidados.RemoveRange(context.TokensInvalidados);
                context.SaveChanges();
                
                logger.LogInformation("Banco de dados limpo para teste");

                // Criar usuário de teste
                var senhaCriptografada = hashService.CriarHash("senha123");
                var usuarioTeste = new Usuario("Usuário Teste", "teste@teste.com", senhaCriptografada);
                usuarioTeste.Ativo = true;
                
                context.Usuarios.Add(usuarioTeste);
                context.SaveChanges();
                
                // Verificar se o usuário foi criado
                var usuarioVerificacao = context.Usuarios.FirstOrDefault(u => u.Email == "teste@teste.com");
                if (usuarioVerificacao == null)
                {
                    logger.LogError("Usuário não encontrado após reset");
                    throw new Exception("Falha ao criar usuário de teste");
                }
                
                logger.LogInformation("Usuário recriado: {Email}, {Nome}, {Id}, {Ativo}",
                    usuarioVerificacao.Email, usuarioVerificacao.Nome, usuarioVerificacao.Id, usuarioVerificacao.Ativo);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao resetar banco de dados");
                throw;
            }
        }
    }
}
