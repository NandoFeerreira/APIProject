using APIProject.Application.Interfaces;
using APIProject.Domain.Interfaces;
using APIProject.Domain.Interfaces.Servicos;
using APIProject.Domain.Servicos;
using APIProject.Infrastructure.Configuracoes;
using APIProject.Infrastructure.Persistencia;
using APIProject.Infrastructure.Persistencia.Repositorios;
using APIProject.Infrastructure.Servicos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace APIProject.Infrastructure.DependencyInjection
{
    public static class InfrastructureServicesExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Se o ambiente for Testing, o banco já foi configurado pelo CustomWebApplicationFactory
            var isTestingEnv = configuration["ASPNETCORE_ENVIRONMENT"] == "Testing";

            // Configurar banco de dados apenas se não for ambiente de teste
            if (!isTestingEnv)
            {
                var useInMemoryDb = configuration.GetValue<bool>("UseInMemoryDatabase");

                // Remove todos os registros relacionados ao DbContext para evitar múltiplos provedores
                var descriptors = services.Where(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                         d.ServiceType == typeof(DbContextOptions) ||
                         d.ServiceType == typeof(ApplicationDbContext));

                foreach (var descriptor in descriptors.ToList())
                {
                    services.Remove(descriptor);
                }

                if (useInMemoryDb)
                {
                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseInMemoryDatabase("APIProjectDb")
                               .EnableServiceProviderCaching(false));
                }
                else
                {
                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseSqlServer(
                            configuration.GetConnectionString("DefaultConnection"),
                            b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
                               .EnableServiceProviderCaching(false));
                }
            }

            // A configuração de JwtConfiguracoes é feita em APIProject.API.Extensions.ServiceCollectionExtensions

            // Registrar serviços de infraestrutura
            services.AddScoped<IHashService, HashService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IUsuarioServico, UsuarioServico>();
            services.AddScoped<ITokenValidacaoServico, TokenValidacaoServico>();

            // Registrar repositórios
            services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
            services.AddScoped<ITokenInvalidadoRepositorio, TokenInvalidadoRepositorio>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Adicionar serviços de domínio
            services.AddDomainServices();

            return services;
        }
    }

}