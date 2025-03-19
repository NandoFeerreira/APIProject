using APIProject.Domain.Entidades;
using APIProject.Domain.Enums;
using APIProject.Domain.Interfaces.Servicos;
using System;

namespace APIProject.Domain.Servicos
{
    public class ProdutoServico : IProdutoServico
    {
        public void AtualizarEstoque(Produto produto, int quantidade)
        {
            if (produto == null)
                throw new ArgumentNullException(nameof(produto));

            if (produto.Estoque + quantidade < 0)
                throw new InvalidOperationException("Estoque insuficiente");

            var novoEstoque = produto.GetType().GetProperty("Estoque");
            novoEstoque.SetValue(produto, produto.Estoque + quantidade);
        }

        public void AtualizarPreco(Produto produto, decimal novoPreco)
        {
            if (produto == null)
                throw new ArgumentNullException(nameof(produto));

            if (novoPreco <= 0)
                throw new ArgumentException("Preço deve ser maior que zero", nameof(novoPreco));

            var preco = produto.GetType().GetProperty("Preco");
            preco.SetValue(produto, novoPreco);
        }

        public void AlterarStatus(Produto produto, StatusProduto novoStatus)
        {
            if (produto == null)
                throw new ArgumentNullException(nameof(produto));

            var status = produto.GetType().GetProperty("Status");
            status.SetValue(produto, novoStatus);
        }

        public void AdicionarCategoria(Produto produto, Categoria categoria)
        {
            if (produto == null)
                throw new ArgumentNullException(nameof(produto));
            
            if (categoria == null)
                throw new ArgumentNullException(nameof(categoria));

            produto.Categorias.Add(categoria);
        }

        public void AdicionarImagem(Produto produto, string urlImagem)
        {
            if (produto == null)
                throw new ArgumentNullException(nameof(produto));
            
            if (string.IsNullOrWhiteSpace(urlImagem))
                throw new ArgumentException("URL da imagem não pode ser vazia", nameof(urlImagem));

            produto.Imagens.Add(urlImagem);
        }
    }
}