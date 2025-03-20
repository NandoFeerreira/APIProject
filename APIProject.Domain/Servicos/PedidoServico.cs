using APIProject.Domain.Entidades;
using APIProject.Domain.Enums;
using APIProject.Domain.Interfaces.Servicos;
using System;
using System.Linq;

namespace APIProject.Domain.Servicos
{
    public class PedidoServico : IPedidoServico
    {
        private readonly IProdutoServico _produtoServico;

        public PedidoServico(IProdutoServico produtoServico)
        {
            _produtoServico = produtoServico ?? throw new ArgumentNullException(nameof(produtoServico));
        }

        public void AdicionarItem(Pedido pedido, Produto produto, int quantidade)
        {
            if (pedido == null)
                throw new ArgumentNullException(nameof(pedido));
            
            if (produto == null)
                throw new ArgumentNullException(nameof(produto));
            
            if (quantidade <= 0)
                throw new ArgumentException("Quantidade deve ser maior que zero", nameof(quantidade));
            
            if (pedido.Status != StatusPedido.Criado)
                throw new InvalidOperationException("Não é possível adicionar itens a um pedido que não está no status Criado");
            
            if (produto.Estoque < quantidade)
                throw new InvalidOperationException("Estoque insuficiente");

            // Verifica se o produto já existe no pedido
            var itemExistente = pedido.Itens.FirstOrDefault(i => i.ProdutoId == produto.Id);
            
            if (itemExistente != null)
            {
                // Atualiza a quantidade do item existente
                var novaQuantidade = itemExistente.Quantidade + quantidade;
                var quantidade_prop = itemExistente.GetType().GetProperty("Quantidade");
                quantidade_prop.SetValue(itemExistente, novaQuantidade);
                
                var subtotal_prop = itemExistente.GetType().GetProperty("Subtotal");
                subtotal_prop.SetValue(itemExistente, itemExistente.PrecoUnitario * novaQuantidade);
            }
            else
            {
                // Adiciona um novo item ao pedido
                var novoItem = new ItemPedido(pedido, produto, quantidade);
                pedido.Itens.Add(novoItem);
            }

            // Atualiza o valor total do pedido
            AtualizarValorTotal(pedido);
        }

        public void RemoverItem(Pedido pedido, Guid itemId)
        {
            if (pedido == null)
                throw new ArgumentNullException(nameof(pedido));
            
            if (pedido.Status != StatusPedido.Criado)
                throw new InvalidOperationException("Não é possível remover itens de um pedido que não está no status Criado");

            var item = pedido.Itens.FirstOrDefault(i => i.Id == itemId);
            
            if (item == null)
                throw new InvalidOperationException("Item não encontrado no pedido");

            pedido.Itens.Remove(item);
            
            // Atualiza o valor total do pedido
            AtualizarValorTotal(pedido);
        }

        public void AtualizarQuantidadeItem(Pedido pedido, Guid itemId, int novaQuantidade)
        {
            if (pedido == null)
                throw new ArgumentNullException(nameof(pedido));
            
            if (novaQuantidade <= 0)
                throw new ArgumentException("Quantidade deve ser maior que zero", nameof(novaQuantidade));
            
            if (pedido.Status != StatusPedido.Criado)
                throw new InvalidOperationException("Não é possível atualizar itens de um pedido que não está no status Criado");

            var item = pedido.Itens.FirstOrDefault(i => i.Id == itemId);
            
            if (item == null)
                throw new InvalidOperationException("Item não encontrado no pedido");

            if (item.Produto.Estoque < novaQuantidade)
                throw new InvalidOperationException("Estoque insuficiente");

            var quantidade_prop = item.GetType().GetProperty("Quantidade");
            quantidade_prop.SetValue(item, novaQuantidade);
            
            var subtotal_prop = item.GetType().GetProperty("Subtotal");
            subtotal_prop.SetValue(item, item.PrecoUnitario * novaQuantidade);
            
            // Atualiza o valor total do pedido
            AtualizarValorTotal(pedido);
        }

        public void AtualizarValorTotal(Pedido pedido)
        {
            if (pedido == null)
                throw new ArgumentNullException(nameof(pedido));

            var valorTotal = pedido.Itens.Sum(i => i.Subtotal);
            
            var valorTotal_prop = pedido.GetType().GetProperty("ValorTotal");
            valorTotal_prop.SetValue(pedido, valorTotal);
        }

        public void AtualizarStatus(Pedido pedido, StatusPedido novoStatus)
        {
            if (pedido == null)
                throw new ArgumentNullException(nameof(pedido));

            // Validações de transição de status
            switch (novoStatus)
            {
                case StatusPedido.Pago:
                    if (pedido.Status != StatusPedido.Criado)
                        throw new InvalidOperationException("Apenas pedidos no status Criado podem ser pagos");
                    
                    // Ao confirmar pagamento, reduzir estoque
                    foreach (var item in pedido.Itens)
                    {
                        _produtoServico.AtualizarEstoque(item.Produto, -item.Quantidade);
                    }
                    break;
                
                case StatusPedido.EmProcessamento:
                    if (pedido.Status != StatusPedido.Pago)
                        throw new InvalidOperationException("Apenas pedidos no status Pago podem entrar em processamento");
                    break;
                
                case StatusPedido.Enviado:
                    if (pedido.Status != StatusPedido.EmProcessamento)
                        throw new InvalidOperationException("Apenas pedidos no status EmProcessamento podem ser enviados");
                    break;
                
                case StatusPedido.Entregue:
                    if (pedido.Status != StatusPedido.Enviado)
                        throw new InvalidOperationException("Apenas pedidos no status Enviado podem ser entregues");
                    break;
                
                case StatusPedido.Cancelado:
                    if (pedido.Status == StatusPedido.Entregue)
                        throw new InvalidOperationException("Pedidos entregues não podem ser cancelados");
                    
                    // Se o pedido já estava pago, devolver ao estoque
                    if (pedido.Status == StatusPedido.Pago || 
                        pedido.Status == StatusPedido.EmProcessamento || 
                        pedido.Status == StatusPedido.Enviado)
                    {
                        foreach (var item in pedido.Itens)
                        {
                            _produtoServico.AtualizarEstoque(item.Produto, item.Quantidade);
                        }
                    }
                    break;
            }

            var status_prop = pedido.GetType().GetProperty("Status");
            status_prop.SetValue(pedido, novoStatus);
        }
    }
}