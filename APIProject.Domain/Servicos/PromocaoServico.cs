using APIProject.Domain.Entidades;
using APIProject.Domain.Interfaces.Servicos;
using System;
using System.Collections.Generic;

namespace APIProject.Domain.Servicos
{
    public class PromocaoServico : IPromocaoServico
    {
        public void AtualizarPromocao(Promocao promocao, string novoNome, string novaDescricao, 
                                     decimal novoPercentualDesconto, DateTime novaDataInicio, 
                                     DateTime novaDataFim)
        {
            if (promocao == null)
                throw new ArgumentNullException(nameof(promocao));
            
            var props = promocao.GetType();
            
            if (!string.IsNullOrWhiteSpace(novoNome))
                props.GetProperty("Nome").SetValue(promocao, novoNome);
            
            if (novaDescricao != null) // Pode ser vazia
                props.GetProperty("Descricao").SetValue(promocao, novaDescricao);
            
            if (novoPercentualDesconto > 0)
            {
                if (novoPercentualDesconto > 100)
                    throw new ArgumentException("Percentual de desconto deve estar entre 0 e 100", nameof(novoPercentualDesconto));
                
                props.GetProperty("PercentualDesconto").SetValue(promocao, novoPercentualDesconto);
            }
            
            if (novaDataInicio != default && novaDataFim != default)
            {
                if (novaDataInicio >= novaDataFim)
                    throw new ArgumentException("Data de início deve ser anterior à data de fim", nameof(novaDataInicio));
                
                props.GetProperty("DataInicio").SetValue(promocao, novaDataInicio);
                props.GetProperty("DataFim").SetValue(promocao, novaDataFim);
                
                // Atualiza o status de ativação
                bool ativa = DateTime.UtcNow >= novaDataInicio && DateTime.UtcNow <= novaDataFim;
                props.GetProperty("Ativa").SetValue(promocao, ativa);
            }
        }

        public void AdicionarProduto(Promocao promocao, Produto produto)
        {
            if (promocao == null)
                throw new ArgumentNullException(nameof(promocao));
            
            if (produto == null)
                throw new ArgumentNullException(nameof(produto));

            if (!promocao.ProdutosAplicaveis.Contains(produto))
                promocao.ProdutosAplicaveis.Add(produto);
        }

        public void RemoverProduto(Promocao promocao, Produto produto)
        {
            if (promocao == null)
                throw new ArgumentNullException(nameof(promocao));
            
            if (produto == null)
                throw new ArgumentNullException(nameof(produto));

            promocao.ProdutosAplicaveis.Remove(produto);
        }

        public void AtualizarStatusAtivacao(Promocao promocao)
        {
            if (promocao == null)
                throw new ArgumentNullException(nameof(promocao));

            var agora = DateTime.UtcNow;
            bool ativa = agora >= promocao.DataInicio && agora <= promocao.DataFim;
            
            var props = promocao.GetType();
            props.GetProperty("Ativa").SetValue(promocao, ativa);
        }

        public decimal CalcularPrecoComDesconto(Produto produto, IEnumerable<Promocao> promocoesAtivas)
        {
            if (produto == null)
                throw new ArgumentNullException(nameof(produto));
            
            if (promocoesAtivas == null)
                return produto.Preco;

            decimal maiorDesconto = 0;
            
            foreach (var promocao in promocoesAtivas)
            {
                if (promocao.Ativa && promocao.ProdutosAplicaveis.Contains(produto))
                {
                    maiorDesconto = Math.Max(maiorDesconto, promocao.PercentualDesconto);
                }
            }
            
            if (maiorDesconto > 0)
            {
                return produto.Preco * (1 - maiorDesconto / 100);
            }
            
            return produto.Preco;
        }
    }
}