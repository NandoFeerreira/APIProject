using APIProject.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace APIProject.Domain.Entidades
{
    public class Pedido
    {
        public Guid Id { get; private set; }
        public Guid ClienteId { get; private set; }
        public DateTime Data { get; private set; }
        public StatusPedido Status { get; private set; }
        public decimal ValorTotal { get; private set; }
        public Guid EnderecoEntregaId { get; private set; }
        public Cliente Cliente { get; private set; }
        public Endereco EnderecoEntrega { get; private set; }
        public ICollection<ItemPedido> Itens { get; private set; }

        protected Pedido() { }

        public Pedido(Cliente cliente, Endereco enderecoEntrega)
        {
            Id = Guid.NewGuid();
            Cliente = cliente ?? throw new ArgumentNullException(nameof(cliente));
            ClienteId = cliente.Id;
            EnderecoEntrega = enderecoEntrega ?? throw new ArgumentNullException(nameof(enderecoEntrega));
            EnderecoEntregaId = enderecoEntrega.Id;
            Data = DateTime.UtcNow;
            Status = StatusPedido.Criado;
            ValorTotal = 0;
            Itens = new List<ItemPedido>();
        }
    }
}