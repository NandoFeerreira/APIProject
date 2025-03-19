using APIProject.Domain.Entidades;
using APIProject.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace APIProject.Domain.Interfaces
{
    public interface IPedidoRepositorio : IRepositorioBase<Pedido>
    {
        Task<IEnumerable<Pedido>> ObterPorClienteAsync(Guid clienteId);
        Task<IEnumerable<Pedido>> ObterPorStatusAsync(StatusPedido status);
        Task<Pedido> ObterComItensProdutosAsync(Guid pedidoId);
    }
}