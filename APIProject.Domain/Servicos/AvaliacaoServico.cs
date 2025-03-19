using APIProject.Domain.Entidades;
using System;
using System.Linq;

namespace APIProject.Domain.Servicos
{
    public class AvaliacaoServico
    {
        public void AtualizarAvaliacao(Avaliacao avaliacao, int novaClassificacao, string novoComentario)
        {
            if (avaliacao == null)
                throw new ArgumentNullException(nameof(avaliacao));
            
            if (novaClassificacao < 1 || novaClassificacao > 5)
                throw new ArgumentException("Classificação deve estar entre 1 e 5", nameof(novaClassificacao));

            var props = avaliacao.GetType();
            props.GetProperty("Classificacao").SetValue(avaliacao, novaClassificacao);
            props.GetProperty("Comentario").SetValue(avaliacao, novoComentario);
            props.GetProperty("Data").SetValue(avaliacao, DateTime.UtcNow); // Atualiza a data da avaliação
        }

        public bool ClienteJaComprou(Cliente cliente, Produto produto)
        {
            if (cliente == null)
                throw new ArgumentNullException(nameof(cliente));
            
            if (produto == null)
                throw new ArgumentNullException(nameof(produto));

            // Verifica se o cliente já comprou o produto
            return cliente.Pedidos
                .Where(p => p.Status == Enums.StatusPedido.Entregue)
                .SelectMany(p => p.Itens)
                .Any(i => i.ProdutoId == produto.Id);
        }

        public bool ClienteJaAvaliou(Cliente cliente, Produto produto)
        {
            if (cliente == null)
                throw new ArgumentNullException(nameof(cliente));
            
            if (produto == null)
                throw new ArgumentNullException(nameof(produto));

            return cliente.Avaliacoes.Any(a => a.ProdutoId == produto.Id);
        }
    }
}