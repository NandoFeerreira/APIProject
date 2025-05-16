using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace APIProject.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Registrar o AutoMapper com os profiles
            services.AddAutoMapper(typeof(ServiceCollectionExtensions).Assembly);
            
            // Registrar MediatR e seus handlers
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));
            
            return services;
        }
    }
}
