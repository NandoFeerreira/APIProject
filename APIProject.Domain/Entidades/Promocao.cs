using System;
using System.Collections.Generic;

namespace APIProject.Domain.Entidades
{
    public class Promocao
    {
        public Guid Id { get; private set; }
        public string Nome { get; private set; }
        public string Descricao { get; private set; }
        public decimal PercentualDesconto { get; private set; }
        public DateTime DataInicio { get; private set; }
        public DateTime DataFim { get; private set; }
        public bool Ativa { get; private set; }
        public ICollection<Produto> ProdutosAplicaveis { get; private set; }

        protected Promocao() { }

        public Promocao(string nome, string descricao, decimal percentualDesconto, 
                       DateTime dataInicio, DateTime dataFim)
        {
            Id = Guid.NewGuid();
            Nome = nome;
            Descricao = descricao;
            
            if (percentualDesconto <= 0 || percentualDesconto > 100)
                throw new ArgumentException("Percentual de desconto deve estar entre 0 e 100", nameof(percentualDesconto));
            
            PercentualDesconto = percentualDesconto;
            
            if (dataInicio >= dataFim)
                throw new ArgumentException("Data de início deve ser anterior à data de fim", nameof(dataInicio));
            
            DataInicio = dataInicio;
            DataFim = dataFim;
            Ativa = DateTime.UtcNow >= dataInicio && DateTime.UtcNow <= dataFim;
            ProdutosAplicaveis = new List<Produto>();
        }
    }
}