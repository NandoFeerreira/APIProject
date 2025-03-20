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

            var props = endereco.GetType();
            
            if (!string.IsNullOrWhiteSpace(logradouro))
                props.GetProperty("Logradouro").SetValue(endereco, logradouro);
            
            if (!string.IsNullOrWhiteSpace(numero))
                props.GetProperty("Numero").SetValue(endereco, numero);
            
            if (complemento != null) // Pode ser vazio
                props.GetProperty("Complemento").SetValue(endereco, complemento);
            
            if (!string.IsNullOrWhiteSpace(bairro))
                props.GetProperty("Bairro").SetValue(endereco, bairro);
            
            if (!string.IsNullOrWhiteSpace(cidade))
                props.GetProperty("Cidade").SetValue(endereco, cidade);
            
            if (!string.IsNullOrWhiteSpace(estado))
                props.GetProperty("Estado").SetValue(endereco, estado);
            
            if (!string.IsNullOrWhiteSpace(cep))
                props.GetProperty("CEP").SetValue(endereco, cep);
        }
    }
}