using APIProject.Domain.Entidades;
using System;

namespace APIProject.Domain.Interfaces.Servicos
{
    public interface IClienteServico
    {
        void AtualizarNome(Cliente cliente, string novoNome);
        void AtualizarEmail(Cliente cliente, string novoEmail);
        void AdicionarEndereco(Cliente cliente, Endereco endereco);
        void DefinirEnderecoPrincipal(Cliente cliente, Guid enderecoId);
    }
}