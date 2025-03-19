using APIProject.Domain.Entidades;
using APIProject.Domain.Enums;
using System;

namespace APIProject.Domain.Interfaces.Servicos
{
    public interface IProdutoServico
    {
        void AtualizarEstoque(Produto produto, int quantidade);
        void AtualizarPreco(Produto produto, decimal novoPreco);
        void AlterarStatus(Produto produto, StatusProduto novoStatus);
        void AdicionarCategoria(Produto produto, Categoria categoria);
        void AdicionarImagem(Produto produto, string urlImagem);
    }
}