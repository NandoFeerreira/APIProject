using APIProject.Domain.Entidades;
using APIProject.Domain.Interfaces.Servicos;
using System;

namespace APIProject.Domain.Servicos
{
    public class CategoriaServico : ICategoriaServico
    {
        public void AtualizarNome(Categoria categoria, string novoNome)
        {
            if (categoria == null)
                throw new ArgumentNullException(nameof(categoria));
            
            if (string.IsNullOrWhiteSpace(novoNome))
                throw new ArgumentException("Nome não pode ser vazio", nameof(novoNome));

            var nome = categoria.GetType().GetProperty("Nome");
            nome.SetValue(categoria, novoNome);
        }

        public void AtualizarDescricao(Categoria categoria, string novaDescricao)
        {
            if (categoria == null)
                throw new ArgumentNullException(nameof(categoria));

            var descricao = categoria.GetType().GetProperty("Descricao");
            descricao.SetValue(categoria, novaDescricao);
        }

        public void AlterarCategoriaPai(Categoria categoria, Categoria novaCategoriaPai)
        {
            if (categoria == null)
                throw new ArgumentNullException(nameof(categoria));

            // Verificar ciclos na hierarquia
            if (novaCategoriaPai != null && VerificarCicloHierarquia(categoria, novaCategoriaPai))
                throw new InvalidOperationException("Esta operação criaria um ciclo na hierarquia de categorias");

            var categoriaPai = categoria.GetType().GetProperty("CategoriaPai");
            categoriaPai.SetValue(categoria, novaCategoriaPai);

            var categoriaPaiId = categoria.GetType().GetProperty("CategoriaPaiId");
            categoriaPaiId.SetValue(categoria, novaCategoriaPai?.Id);
        }

        private bool VerificarCicloHierarquia(Categoria categoria, Categoria potencialPai)
        {
            if (categoria.Id == potencialPai.Id)
                return true;

            var atual = potencialPai.CategoriaPai;
            while (atual != null)
            {
                if (atual.Id == categoria.Id)
                    return true;
                atual = atual.CategoriaPai;
            }

            return false;
        }

        public void AdicionarSubcategoria(Categoria categoria, Categoria subcategoria)
        {
            if (categoria == null)
                throw new ArgumentNullException(nameof(categoria));
            
            if (subcategoria == null)
                throw new ArgumentNullException(nameof(subcategoria));

            // Verificar ciclos na hierarquia
            if (VerificarCicloHierarquia(categoria, subcategoria))
                throw new InvalidOperationException("Esta operação criaria um ciclo na hierarquia de categorias");

            categoria.SubCategorias.Add(subcategoria);
            
            var categoriaPai = subcategoria.GetType().GetProperty("CategoriaPai");
            categoriaPai.SetValue(subcategoria, categoria);
            
            var categoriaPaiId = subcategoria.GetType().GetProperty("CategoriaPaiId");
            categoriaPaiId.SetValue(subcategoria, categoria.Id);
        }
    }
}