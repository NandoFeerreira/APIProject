using APIProject.API;
using APIProject.Application.Extensions;
using APIProject.Infrastructure.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using APIProject.Infrastructure.Persistencia;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
                    builder.ConfigureAppConfiguration((context, config) =>
                    {
                        config.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.Testing.json"));
                    });
                    builder.ConfigureServices(services =>
                    {
                        var configuration = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.Testing.json", optional: false)
                            .Build();

                        services.AddSingleton<IConfiguration>(configuration);
                        // Remove todos os registros relacionados ao DbContext e seus provedores
                        var descriptors = services.Where(
                            d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                                 d.ServiceType == typeof(DbContextOptions) ||
                                 d.ServiceType == typeof(ApplicationDbContext));

                        foreach (var descriptor in descriptors.ToList())
                        {
                            services.Remove(descriptor);
                        }
                        
                        // Configura o banco em memória para testes
                        services.AddDbContext<ApplicationDbContext>(options =>
                        {
                            options.UseInMemoryDatabase("TestingDb")
                                  .EnableServiceProviderCaching(false)
                                  .EnableDetailedErrors()
                                  .EnableSensitiveDataLogging();
                        });

                        // Adiciona serviços da aplicação
                        services.AddApplicationLayer();

                        // Configurar JWT para testes usando a configuração existente

                        var sp = services.BuildServiceProvider();

                        using (var scope = sp.CreateScope())
                        {
                            var scopedServices = scope.ServiceProvider;
                            var db = scopedServices.GetRequiredService<ApplicationDbContext>();

                            db.Database.EnsureCreated();

                            // Adicionar usuário de teste para autenticação
                            db.Usuarios.RemoveRange(db.Usuarios);
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