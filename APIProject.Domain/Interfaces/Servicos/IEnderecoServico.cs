using APIProject.Domain.Entidades;
using System;

namespace APIProject.Domain.Interfaces.Servicos
{
    public interface IEnderecoServico
    {
        void AtualizarEndereco(Endereco endereco, string logradouro, string numero, 
                              string bairro, string cidade, string estado, 
                              string cep, string complemento = "");
    }
}