using APIProject.Domain.Entidades;
using System;
using System.Collections.Generic;

namespace APIProject.Domain.Interfaces.Servicos
{
    public interface IPromocaoServico
    {
        void AtualizarPromocao(Promocao promocao, string novoNome, string novaDescricao, 
                              decimal novoPercentualDesconto, DateTime novaDataInicio, 
                              DateTime novaDataFim);
        void AdicionarProduto(Promocao promocao, Produto produto);
        void RemoverProduto(Promocao promocao, Produto produto);
        void AtualizarStatusAtivacao(Promocao promocao);
        decimal CalcularPrecoComDesconto(Produto produto, IEnumerable<Promocao> promocoesAtivas);
    }
}