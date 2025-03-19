using APIProject.Domain.Enums;
using System;
using System.Collections.Generic;

namespace APIProject.Domain.Entidades
{
    public class Produto
    {
        public Guid Id { get; private set; }
        public string Nome { get; private set; }
        public string Descricao { get; private set; }
        public decimal Preco { get; private set; }
        public int Estoque { get; private set; }
        public StatusProduto Status { get; private set; }
        public ICollection<string> Imagens { get; private set; }
        public ICollection<Categoria> Categorias { get; private set; }
        public ICollection<Avaliacao> Avaliacoes { get; private set; }

        protected Produto() { }

        public Produto(string nome, string descricao, decimal preco, int estoque)
        {
            Id = Guid.NewGuid();
            Nome = nome;
            Descricao = descricao;
            Preco = preco;
            Estoque = estoque;
            Status = StatusProduto.Ativo;
            Imagens = new List<string>();
            Categorias = new List<Categoria>();
            Avaliacoes = new List<Avaliacao>();
        }
    }
}