using APIProject.Domain.Interfaces.Servicos;
using APIProject.Domain.Servicos;
using Microsoft.Extensions.DependencyInjection;

namespace APIProject.Infrastructure.DependencyInjection
{
    public static class DomainServicesExtensions
    {
        public static IServiceCollection AddDomainServices(this IServiceCollection services)
        {
            // Adicione aqui outros serviços de domínio que não estão em InfrastructureServicesExtensions
            // O IUsuarioServico já está registrado em InfrastructureServicesExtensions

            return services;
        }
    }
}