using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using APIProject.Infrastructure.DependencyInjection;
using APIProject.Application.Extensions;

namespace APIProject.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
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