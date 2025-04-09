using APIProject.Infrastructure.Persistencia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;

namespace APIProject.IntegrationTests
{
    /// <summary>
    /// Fixture compartilhada para testes de integração
    /// </summary>
    public class TestFixture : IDisposable
    {
        public CustomWebApplicationFactory Factory { get; }
        public HttpClient Client { get; }
        private readonly string _databaseName;
        private bool _disposed;

        // Usar o mesmo nome de banco de dados para todos os testes
        private static readonly string _sharedDatabaseName = "TestDb-Shared";

        public TestFixture()
        {
            _databaseName = _sharedDatabaseName;
            Factory = new CustomWebApplicationFactory(_databaseName);
            Factory.SeedTestData();
            Client = Factory.CreateClient();
        }

        /// <summary>
        /// Limpa o banco de dados e recarrega os dados de teste
        /// </summary>
        public void ResetDatabase()
        {
            // Clear existing data and reseed
            using var scope = Factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<TestFixture>>();

            // Limpar todos os dados existentes
            logger.LogInformation("Limpando banco de dados para teste");
            context.Usuarios.RemoveRange(context.Usuarios);
            context.RefreshTokens.RemoveRange(context.RefreshTokens);
            context.TokensInvalidados.RemoveRange(context.TokensInvalidados);
            context.SaveChanges();

            // Recriar os dados de teste
            Factory.SeedTestData();

            // Verificar se os dados foram criados corretamente
            var usuario = context.Usuarios.FirstOrDefault(u => u.Email == "teste@teste.com");
            if (usuario != null)
            {
                logger.LogInformation("Usuário de teste verificado após reset: {Email}, {Nome}, Ativo: {Ativo}",
                    usuario.Email, usuario.Nome, usuario.Ativo);
            }
            else
            {
                logger.LogWarning("Usuário de teste não encontrado após reset");
            }
        }

        /// <summary>
        /// Define o token de autenticação para as requisições
        /// </summary>
        public void SetAuthenticationToken(string token)
        {
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Client.Dispose();
                Factory.Dispose();
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }
    }
}