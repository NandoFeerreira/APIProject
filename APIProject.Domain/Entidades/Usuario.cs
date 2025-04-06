using System;
using System.Collections.Generic;

namespace APIProject.Domain.Entidades
{
    public class Usuario
    {
        public Guid Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Senha { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? UltimoLogin { get; set; }
        public bool Ativo { get; set; }
       
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

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
        }

        // M�todo para adicionar um novo refresh token ao usu�rio
        public void AdicionarRefreshToken(string token, DateTime dataExpiracao)
        {
            RefreshTokens.Add(new RefreshToken
            {
                Token = token,
                DataExpiracao = dataExpiracao,
                UsuarioId = Id
            });
        }
    }
}
