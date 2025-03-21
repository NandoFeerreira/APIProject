using APIProject.Application.Interfaces;
using APIProject.Domain.Interfaces;
using APIProject.Infrastructure.Configuracoes;
using APIProject.Infrastructure.Persistencia;
using APIProject.Infrastructure.Persistencia.Repositorios;
using APIProject.Infrastructure.Servicos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace APIProject.Infrastructure.DependencyInjection
{
    public static class InfrastructureServicesExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configurar banco de dados
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

            // Configurar JWT
            var jwtSecao = configuration.GetSection("JwtConfiguracoes");
            services.Configure<JwtConfiguracoes>(jwtSecao);

            var jwtConfiguracoes = jwtSecao.Get<JwtConfiguracoes>();
            var chave = Encoding.ASCII.GetBytes(jwtConfiguracoes.Chave);

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(chave),
                    ValidateIssuer = true,
                    ValidIssuer = jwtConfiguracoes.Emissor,
                    ValidateAudience = true,
                    ValidAudience = jwtConfiguracoes.Audiencia,
                    ValidateLifetime = true
                };
            });

            // Registrar serviços de infraestrutura
            services.AddScoped<IHashService, HashService>();
            services.AddScoped<ITokenService, TokenService>();

            // Registrar repositórios
            services.AddScoped(typeof(IRepositorioBase<>), typeof(RepositorioBase<>));
            services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();

            return services;
        }
    }
}