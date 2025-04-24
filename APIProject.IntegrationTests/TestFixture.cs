using APIProject.Infrastructure.Persistencia;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace APIProject.IntegrationTests
{
    public class TestFixture
    {
        public readonly CustomWebApplicationFactory Factory;
        public readonly HttpClient Client;
        private readonly ILogger<TestFixture> _logger;

        public TestFixture()
        {
            Factory = new CustomWebApplicationFactory();
            _logger = Factory.Services.GetRequiredService<ILogger<TestFixture>>();
            
            try
            {
                Client = Factory.CreateClient(new WebApplicationFactoryClientOptions
                {
                    AllowAutoRedirect = false
                });

                // Inicializar o banco de dados com dados de teste
                Factory.SeedTestData();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao inicializar o TestFixture");
                throw;
            }
        }

        // Método para limpar e recriar o banco de dados
        public void ResetDatabase()
        {
            _logger.LogInformation("Resetando banco de dados para testes");
            Factory.ResetDatabase();
        }

        // Método para verificar se o serviço de banco de dados está acessível
        public bool VerificarBancoDeDados()
        {
            try
            {
                using var scope = Factory.Services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                return context.Database.CanConnect();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar conexão com o banco de dados");
                return false;
            }
        }
    }
}