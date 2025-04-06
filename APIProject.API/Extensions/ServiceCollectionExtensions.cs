using APIProject.Application.DTOs;
using APIProject.Application.Extensions;
using APIProject.Application.Usuarios.Comandos.Consulta;
using APIProject.Application.Usuarios.Comandos.LoginUsuario;
using APIProject.Application.Usuarios.Comandos.Logout;
using APIProject.Application.Usuarios.Comandos.RefreshToken;
using APIProject.Application.Usuarios.Comandos.RegistrarUsuario;
using APIProject.Infrastructure.Configuracoes;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json;

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

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        logger.LogError($"Autenticação JWT falhou: {context.Exception.Message}");
                        return Task.CompletedTask;
                    },
                    OnMessageReceived = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();

                        // Log do header Authorization
                        var authHeader = context.HttpContext.Request.Headers["Authorization"].ToString();
                        logger.LogInformation($"Header Authorization: {authHeader}");

                        // Se o token for nulo mas o header existir, extraímos manualmente
                        if (string.IsNullOrEmpty(context.Token) && !string.IsNullOrEmpty(authHeader))
                        {
                            if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                            {
                                var token = authHeader.Substring("Bearer ".Length).Trim();
                                context.Token = token;
                                logger.LogInformation($"Token extraído manualmente: {token.Substring(0, Math.Min(10, token.Length))}...");
                            }
                            else
                            {
                                logger.LogWarning("Header Authorization não começa com 'Bearer '");
                            }
                        }

                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        logger.LogInformation("Token JWT validado com sucesso");
                        return Task.CompletedTask;
                    }
                };
            });

            // Configuração de autorização
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
