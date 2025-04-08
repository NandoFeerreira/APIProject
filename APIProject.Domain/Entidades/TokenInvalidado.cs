
using System;

namespace APIProject.Domain.Entidades
{
    public class TokenInvalidado
    {
        public Guid Id { get; set; }
        public string Jti { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty; 
        public Guid UsuarioId { get; set; } 
        public DateTime DataExpiracao { get; set; }
        public DateTime DataInvalidacao { get; set; } 

        public virtual Usuario Usuario { get; set; } = null!;

        public TokenInvalidado()
        {
            Id = Guid.NewGuid();
            DataInvalidacao = DateTime.UtcNow;
        }

        public bool EstaExpirado => DateTime.UtcNow >= DataExpiracao;
    }
}
