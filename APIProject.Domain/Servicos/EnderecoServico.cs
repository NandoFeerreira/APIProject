using APIProject.Domain.Entidades;
using APIProject.Domain.Interfaces.Servicos;
using System;

namespace APIProject.Domain.Servicos
{
    public class EnderecoServico : IEnderecoServico
    {
        public void AtualizarEndereco(Endereco endereco, string logradouro, string numero, 
                                     string bairro, string cidade, string estado, 
                                     string cep, string complemento = null)
        {
            if (endereco == null)
                throw new ArgumentNullException(nameof(endereco));

            var type = typeof(Endereco);
            
            if (!string.IsNullOrWhiteSpace(logradouro))
                type.GetProperty(nameof(Endereco.Logradouro)).SetValue(endereco, logradouro);
            
            if (!string.IsNullOrWhiteSpace(numero))
                type.GetProperty(nameof(Endereco.Numero)).SetValue(endereco, numero);
            
            if (complemento != null) // Pode ser vazio
                type.GetProperty(nameof(Endereco.Complemento)).SetValue(endereco, complemento);
            
            if (!string.IsNullOrWhiteSpace(bairro))
                type.GetProperty(nameof(Endereco.Bairro)).SetValue(endereco, bairro);
            
            if (!string.IsNullOrWhiteSpace(cidade))
                type.GetProperty(nameof(Endereco.Cidade)).SetValue(endereco, cidade);
            
            if (!string.IsNullOrWhiteSpace(estado))
                type.GetProperty(nameof(Endereco.Estado)).SetValue(endereco, estado);
            
            if (!string.IsNullOrWhiteSpace(cep))
                type.GetProperty(nameof(Endereco.CEP)).SetValue(endereco, cep);
        }
    }
}