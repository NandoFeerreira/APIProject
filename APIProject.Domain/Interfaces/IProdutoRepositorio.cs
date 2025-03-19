using APIProject.Domain.Entidades;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace APIProject.Domain.Interfaces
{
    public interface IProdutoRepositorio : IRepositorioBase<Produto>
    {
        Task<IEnumerable<Produto>> ObterPorCategoriaAsync(Guid categoriaId);
        Task<IEnumerable<Produto>> ObterPorNomeAsync(string nome);
        Task<IEnumerable<Produto>> ObterProdutosComEstoqueBaixoAsync(int limiteEstoque);
        Task<IEnumerable<Produto>> ObterProdutosComPromocaoAtivaAsync();
    }
}