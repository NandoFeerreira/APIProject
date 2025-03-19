using System;

namespace APIProject.Domain.Entidades
{
    public class Endereco
    {
        public Guid Id { get; private set; }
        public Guid ClienteId { get; private set; }
        public string Logradouro { get; private set; }
        public string Numero { get; private set; }
        public string Complemento { get; private set; }
        public string Bairro { get; private set; }
        public string Cidade { get; private set; }
        public string Estado { get; private set; }
        public string CEP { get; private set; }
        public bool Principal { get; private set; }
        public Cliente Cliente { get; private set; }

        protected Endereco() { }

        public Endereco(Cliente cliente, string logradouro, string numero, string bairro, 
                       string cidade, string estado, string cep, string complemento = null)
        {
            Id = Guid.NewGuid();
            Cliente = cliente ?? throw new ArgumentNullException(nameof(cliente));
            ClienteId = cliente.Id;
            Logradouro = logradouro;
            Numero = numero;
            Complemento = complemento;
            Bairro = bairro;
            Cidade = cidade;
            Estado = estado;
            CEP = cep;
            Principal = cliente.Enderecos.Count == 0; // Primeiro endereço é o principal
        }
    }
}