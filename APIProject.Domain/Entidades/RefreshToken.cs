// APIProject.Domain/Entidades/RefreshToken.cs
using System;

namespace APIProject.Domain.Entidades
{
    public class RefreshToken
    {
        public Guid Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; }
        public DateTime DataExpiracao { get; set; }
        public bool Utilizado { get; set; }
        public bool Invalidado { get; set; }
        public Guid UsuarioId { get; set; }
        public virtual Usuario Usuario { get; set; } = null!;

        public RefreshToken()
        {
            DataCriacao = DateTime.UtcNow;
        }

       
        public bool EstaExpirado => DateTime.UtcNow >= DataExpiracao;
   
        public bool EstaAtivo => !Utilizado && !Invalidado && !EstaExpirado;
    }
}
