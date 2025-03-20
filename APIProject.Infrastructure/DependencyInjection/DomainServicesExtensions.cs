using APIProject.Domain.Interfaces.Servicos;
using APIProject.Domain.Servicos;
using Microsoft.Extensions.DependencyInjection;

namespace APIProject.Infrastructure.DependencyInjection
{
    public static class DomainServicesExtensions
    {
        public static IServiceCollection AddDomainServices(this IServiceCollection services)
        {            
            services.AddScoped<IProdutoServico, ProdutoServico>();
            services.AddScoped<ICategoriaServico, CategoriaServico>();
            services.AddScoped<IClienteServico, ClienteServico>();
            services.AddScoped<IEnderecoServico, EnderecoServico>();
            services.AddScoped<IPedidoServico, PedidoServico>();
            services.AddScoped<IAvaliacaoServico, AvaliacaoServico>();
            services.AddScoped<IPromocaoServico, PromocaoServico>();

            return services;
        }
    }
}