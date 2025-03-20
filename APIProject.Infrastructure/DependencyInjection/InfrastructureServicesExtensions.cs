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
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

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