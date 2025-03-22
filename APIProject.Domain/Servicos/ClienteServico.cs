using APIProject.Domain.Entidades;
using APIProject.Domain.Interfaces.Servicos;
using System;

namespace APIProject.Domain.Servicos
{
    public class ClienteServico : IClienteServico
    {
        public void AtualizarNome(Cliente cliente, string novoNome)
        {
            if (cliente == null)
                throw new ArgumentNullException(nameof(cliente));
            
            if (string.IsNullOrWhiteSpace(novoNome))
                throw new ArgumentException("Nome não pode ser vazio", nameof(novoNome));

            var type = typeof(Cliente);
            type.GetProperty(nameof(Cliente.Nome)).SetValue(cliente, novoNome);
        }

        public void AtualizarEmail(Cliente cliente, string novoEmail)
        {
            if (cliente == null)
                throw new ArgumentNullException(nameof(cliente));
            
            if (string.IsNullOrWhiteSpace(novoEmail))
                throw new ArgumentException("Email não pode ser vazio", nameof(novoEmail));

            var type = typeof(Cliente);
            type.GetProperty(nameof(Cliente.Email)).SetValue(cliente, novoEmail);
        }

        public void AdicionarEndereco(Cliente cliente, Endereco endereco)
        {
            if (cliente == null)
                throw new ArgumentNullException(nameof(cliente));
            
            if (endereco == null)
                throw new ArgumentNullException(nameof(endereco));

            cliente.Enderecos.Add(endereco);
        }

        public void DefinirEnderecoPrincipal(Cliente cliente, Guid enderecoId)
        {
            if (cliente == null)
                throw new ArgumentNullException(nameof(cliente));

            var enderecoPrincipal = cliente.Enderecos.FirstOrDefault(e => e.Id == enderecoId);
            
            if (enderecoPrincipal == null)
                throw new InvalidOperationException("Endereço não encontrado");

            foreach (var endereco in cliente.Enderecos)
            {
                var type = typeof(Endereco);
                type.GetProperty(nameof(Endereco.Principal)).SetValue(endereco, endereco.Id == enderecoId);
            }
        }
    }
}