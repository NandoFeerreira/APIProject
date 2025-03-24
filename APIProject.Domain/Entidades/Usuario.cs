using System;
using System.Collections.Generic;

namespace APIProject.Domain.Entidades
{
    public class Usuario
    {
        public Guid Id { get; private set; }
        public string Nome { get; private set; }
        public string Email { get; private set; }
        public string Senha { get; private set; }
        public DateTime DataCriacao { get; private set; }
        public DateTime? UltimoLogin { get; private set; }
        public bool Ativo { get; private set; }
        public ICollection<string> Perfis { get; private set; }

        protected Usuario() { }

        public void Desativar()
        {
            Ativo = false;
        }

        public Usuario(string nome, string email, string senhaCriptografada)
        {
            Id = Guid.NewGuid();
            Nome = nome;
            Email = email;
            Senha = senhaCriptografada;
            DataCriacao = DateTime.UtcNow;
            Ativo = true;
            Perfis = new List<string> { "Usuario" };
        }
    }
}