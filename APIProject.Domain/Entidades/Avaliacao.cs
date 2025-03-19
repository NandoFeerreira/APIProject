using System;

namespace APIProject.Domain.Entidades
{
    public class Avaliacao
    {
        public Guid Id { get; private set; }
        public Guid ProdutoId { get; private set; }
        public Guid ClienteId { get; private set; }
        public int Classificacao { get; private set; }
        public string Comentario { get; private set; }
        public DateTime Data { get; private set; }
        public Produto Produto { get; private set; }
        public Cliente Cliente { get; private set; }

        protected Avaliacao() { }

        public Avaliacao(Produto produto, Cliente cliente, int classificacao, string comentario)
        {
            Id = Guid.NewGuid();
            Produto = produto ?? throw new ArgumentNullException(nameof(produto));
            ProdutoId = produto.Id;
            Cliente = cliente ?? throw new ArgumentNullException(nameof(cliente));
            ClienteId = cliente.Id;
            
            if (classificacao < 1 || classificacao > 5)
                throw new ArgumentException("Classificação deve estar entre 1 e 5", nameof(classificacao));
            
            Classificacao = classificacao;
            Comentario = comentario;
            Data = DateTime.UtcNow;
        }
    }
}