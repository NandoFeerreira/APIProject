using System;
using System.Collections.Generic;

namespace APIProject.Domain.Entidades
{
    public class Cliente
    {
        public Guid Id { get; private set; }
        public string Nome { get; private set; }
        public string Email { get; private set; }
        public DateTime DataRegistro { get; private set; }
        public ICollection<Endereco> Enderecos { get; private set; }
        public ICollection<Pedido> Pedidos { get; private set; }
        public ICollection<Avaliacao> Avaliacoes { get; private set; }

        protected Cliente() { }

        public Cliente(string nome, string email)
        {
            Id = Guid.NewGuid();
            Nome = nome;
            Email = email;
            DataRegistro = DateTime.UtcNow;
            Enderecos = new List<Endereco>();
            Pedidos = new List<Pedido>();
            Avaliacoes = new List<Avaliacao>();
        }
    }
}