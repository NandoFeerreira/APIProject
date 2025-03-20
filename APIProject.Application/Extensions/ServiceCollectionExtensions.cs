using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using AutoMapper;

namespace APIProject.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
        {
            // Registrar o AutoMapper com os profiles
            services.AddAutoMapper(cfg => cfg.AddMaps(Assembly.GetExecutingAssembly()));
            
            // Aqui podemos adicionar outros serviços da camada de aplicação
            
            return services;
        }
    }
}