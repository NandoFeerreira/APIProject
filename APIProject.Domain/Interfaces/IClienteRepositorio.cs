using APIProject.Domain.Entidades;
using System;
using System.Threading.Tasks;

namespace APIProject.Domain.Interfaces
{
    public interface IClienteRepositorio : IRepositorioBase<Cliente>
    {
        Task<Cliente> ObterPorEmailAsync(string email);
        Task<Cliente> ObterComEnderecosPedidosAsync(Guid clienteId);
    }
}