using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using APIProject.Infrastructure.DependencyInjection;
using APIProject.Application.Extensions;

namespace APIProject.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOpenApi(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "API Project",
                    Version = "v1",
                    Description = "API Project with .NET Core"
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
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
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header
                        },
                        new List<string>()
                    }
                });
            });

            return services;
        }

        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Registrar serviços do domínio através da camada de infraestrutura
            services.AddDomainServices();
            
            // Registrar serviços da camada de aplicação
            services.AddApplicationLayer();
            
            // Registrar serviços da camada de infraestrutura
            services.AddInfrastructureServices(configuration);
            
            return services;
        }
    }
}