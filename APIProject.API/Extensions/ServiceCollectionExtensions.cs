using APIProject.Application.Extensions;
using APIProject.Application.Usuarios.Comandos.LoginUsuario;
using APIProject.Application.Usuarios.Comandos.RefreshToken;
using APIProject.Application.Usuarios.Comandos.RegistrarUsuario;
using APIProject.Infrastructure.Configuracoes;
using APIProject.Infrastructure.Servicos;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace APIProject.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Adiciona serviços da camada de aplicação
            services.AddApplicationLayer();

            services.AddScoped<IValidator<LoginUsuarioComando>, LoginUsuarioComandoValidador>();
            services.AddScoped<IValidator<RegistrarUsuarioComando>, RegistrarUsuarioComandoValidador>();
            services.AddScoped<IValidator<RefreshTokenComando>, RefreshTokenComandoValidador>();            

            services.AddHostedService<TokenLimpezaBackgroundService>();

            // Configuração JWT clara e explícita
            var jwtConfiguracoes = configuration.GetSection("JwtConfiguracoes");

            services.Configure<JwtConfiguracoes>(jwtConfiguracoes);

            var jwtConfig = jwtConfiguracoes.Get<JwtConfiguracoes>();

            if (string.IsNullOrEmpty(jwtConfig!.Chave) ||
                string.IsNullOrEmpty(jwtConfig.Emissor) ||
                string.IsNullOrEmpty(jwtConfig.Audiencia))
            {
                throw new InvalidOperationException("Configurações JWT incompletas ou inválidas");
            }

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["JwtConfiguracoes:Emissor"],
                    ValidAudience = configuration["JwtConfiguracoes:Audiencia"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["JwtConfiguracoes:Chave"]!))
                };               
            });

            
            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });

            return services;
        }
    }
}
