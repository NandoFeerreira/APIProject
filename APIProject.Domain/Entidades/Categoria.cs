using System;
using System.Collections.Generic;

namespace APIProject.Domain.Entidades
{
    public class Categoria
    {
        public Guid Id { get; private set; }
        public string Nome { get; private set; }
        public string Descricao { get; private set; }
        public Guid? CategoriaPaiId { get; private set; }
        public Categoria CategoriaPai { get; private set; }
        public ICollection<Categoria> SubCategorias { get; private set; }
        public ICollection<Produto> Produtos { get; private set; }

        protected Categoria() { }

        public Categoria(string nome, string descricao, Categoria categoriaPai = null)
        {
            Id = Guid.NewGuid();
            Nome = nome;
            Descricao = descricao;
            
            if (categoriaPai != null)
            {
                CategoriaPai = categoriaPai;
                CategoriaPaiId = categoriaPai.Id;
            }
            
            SubCategorias = new List<Categoria>();
            Produtos = new List<Produto>();
        }
    }
}