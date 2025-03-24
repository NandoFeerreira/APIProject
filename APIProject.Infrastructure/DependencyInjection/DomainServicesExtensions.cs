using APIProject.Domain.Interfaces.Servicos;
using APIProject.Domain.Servicos;
using Microsoft.Extensions.DependencyInjection;

namespace APIProject.Infrastructure.DependencyInjection
{
    public static class DomainServicesExtensions
    {
        public static IServiceCollection AddDomainServices(this IServiceCollection services)
        {            
           
            services.AddScoped<IUsuarioServico, UsuarioServico>();

            return services;
        }
    }
}