using APIProject.Domain.Entidades;
using APIProject.Domain.Enums;
using System;

namespace APIProject.Domain.Interfaces.Servicos
{
    public interface IPedidoServico
    {
        void AdicionarItem(Pedido pedido, Produto produto, int quantidade);
        void RemoverItem(Pedido pedido, Guid itemId);
        void AtualizarQuantidadeItem(Pedido pedido, Guid itemId, int novaQuantidade);
        void AtualizarValorTotal(Pedido pedido);
        void AtualizarStatus(Pedido pedido, StatusPedido novoStatus);
    }
}