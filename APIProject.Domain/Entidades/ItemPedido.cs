using System;

namespace APIProject.Domain.Entidades
{
    public class ItemPedido
    {
        public Guid Id { get; private set; }
        public Guid PedidoId { get; private set; }
        public Guid ProdutoId { get; private set; }
        public int Quantidade { get; private set; }
        public decimal PrecoUnitario { get; private set; }
        public decimal Subtotal { get; private set; }
        public Pedido Pedido { get; private set; }
        public Produto Produto { get; private set; }

        protected ItemPedido() { }

        public ItemPedido(Pedido pedido, Produto produto, int quantidade)
        {
            Id = Guid.NewGuid();
            Pedido = pedido ?? throw new ArgumentNullException(nameof(pedido));
            PedidoId = pedido.Id;
            Produto = produto ?? throw new ArgumentNullException(nameof(produto));
            ProdutoId = produto.Id;
            
            if (quantidade <= 0)
                throw new ArgumentException("Quantidade deve ser maior que zero", nameof(quantidade));
            
            Quantidade = quantidade;
            PrecoUnitario = produto.Preco; // Snapshot do preço no momento da compra
            Subtotal = PrecoUnitario * quantidade;
        }
    }
}