using System;
using System.Collections.Generic;

namespace APIProject.Domain.Entidades
{
    public class Usuario
    {
        public Guid Id { get;  set; }
        public string Nome { get;  set; }
        public string Email { get;  set; }
        public string Senha { get;  set; }
        public DateTime DataCriacao { get;  set; }
        public DateTime? UltimoLogin { get;  set; }
        public bool Ativo { get;  set; }
        public ICollection<string> Perfis { get;  set; }

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