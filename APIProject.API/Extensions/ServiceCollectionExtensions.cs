using APIProject.Application.Extensions;
using APIProject.Application.Usuarios.Comandos.LoginUsuario;
using APIProject.Application.Usuarios.Comandos.RegistrarUsuario;
using FluentValidation;
using Microsoft.OpenApi.Models;

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


            return services;
        }

        public static IServiceCollection AddOpenApi(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "API Project",
                    Version = "v1",
                    Description = "API para o projeto APIProject"
                });

                // Configuração para autenticação JWT no Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                         },
                        Array.Empty<string>()
                     }
                 });
            });

            return services;
        }



    }
}