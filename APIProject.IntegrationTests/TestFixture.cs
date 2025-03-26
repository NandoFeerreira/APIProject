using APIProject.Infrastructure.Persistencia;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;

namespace APIProject.IntegrationTests
{

    public class TestFixture : IDisposable
    {
        public CustomWebApplicationFactory Factory { get; }
        public HttpClient Client { get; }
        private readonly string _databaseName;
        private bool _disposed;

        public TestFixture()
        {
            _databaseName = $"TestDb-{Guid.NewGuid()}";
            Factory = new CustomWebApplicationFactory(_databaseName);
            Factory.SeedTestData();
            Client = Factory.CreateClient();
        }

        public void ResetDatabase()
        {
            // Clear existing data and reseed
            using var scope = Factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            Factory.SeedTestData();
        }

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
        }


    }
}