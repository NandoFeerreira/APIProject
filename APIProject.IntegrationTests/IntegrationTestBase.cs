using APIProject.API;
using APIProject.Infrastructure.Persistencia;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;

namespace APIProject.IntegrationTests
{
    public class IntegrationTestBase
    {
        protected readonly WebApplicationFactory<Program> _factory;
        protected readonly HttpClient _client;

        public IntegrationTestBase()
        {
            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        // Remove todos os registros relacionados ao DbContext e seus provedores
                        var descriptors = services.Where(
                            d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                                 d.ServiceType == typeof(DbContextOptions) ||
                                 d.ServiceType == typeof(ApplicationDbContext));

                        foreach (var descriptor in descriptors.ToList())
                        {
                            services.Remove(descriptor);
                        }

                        // Remove registros específicos dos provedores de banco de dados
                        var dbProviderDescriptors = services.Where(
                            d => d.ServiceType.Namespace?.StartsWith("Microsoft.EntityFrameworkCore") == true &&
                                 (d.ServiceType.Name.Contains("SqlServer") || d.ServiceType.Name.Contains("InMemory")));

                        foreach (var descriptor in dbProviderDescriptors.ToList())
                        {
                            services.Remove(descriptor);
                        }
                        
                        // Adiciona apenas o banco em memória para testes
                        services.AddDbContext<ApplicationDbContext>(options =>
                        {
                            options.UseInMemoryDatabase("TestDb")
                                  .EnableServiceProviderCaching(false);
                        });

                        var sp = services.BuildServiceProvider();

                        using (var scope = sp.CreateScope())
                        {
                            var scopedServices = scope.ServiceProvider;
                            var db = scopedServices.GetRequiredService<ApplicationDbContext>();

                            db.Database.EnsureCreated();

                            // Adicionar usuário de teste para autenticação
                            var usuario = new APIProject.Domain.Entidades.Usuario("Usuário Teste", "teste@teste.com", "senha123");
                            db.Usuarios.Add(usuario);
                            db.SaveChanges();
                        }
                    });
                });

            _client = _factory.CreateClient();
        }

        protected void SetAuthenticationToken(string token)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        // protected void SeedTestData(ApplicationDbContext context)
        // {
        //     // Adicione dados de teste aqui
        // }
    }
}