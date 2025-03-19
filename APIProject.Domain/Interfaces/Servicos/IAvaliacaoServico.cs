using APIProject.Domain.Entidades;
using System;

namespace APIProject.Domain.Interfaces.Servicos
{
    public interface IAvaliacaoServico
    {
        void AtualizarAvaliacao(Avaliacao avaliacao, int novaClassificacao, string novoComentario);
        bool ClienteJaComprou(Cliente cliente, Produto produto);
        bool ClienteJaAvaliou(Cliente cliente, Produto produto);
    }
}