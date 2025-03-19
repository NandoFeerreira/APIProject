using APIProject.Domain.Entidades;
using System;

namespace APIProject.Domain.Interfaces.Servicos
{
    public interface ICategoriaServico
    {
        void AtualizarNome(Categoria categoria, string novoNome);
        void AtualizarDescricao(Categoria categoria, string novaDescricao);
        void AlterarCategoriaPai(Categoria categoria, Categoria novaCategoriaPai);
        void AdicionarSubcategoria(Categoria categoria, Categoria subcategoria);
    }
}