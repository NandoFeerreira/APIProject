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

        public bool EstaExpirado => DateTime.UtcNow >= DataExpiracao;

        public TokenInvalidado()
        {
            Id = Guid.NewGuid();
            DataInvalidacao = DateTime.UtcNow;
        }

        public TokenInvalidado(string jti, string token, Guid usuarioId, DateTime dataExpiracao)
        {
            if (string.IsNullOrWhiteSpace(jti))
                throw new ArgumentException("O JTI não pode ser vazio", nameof(jti));

            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("O token não pode ser vazio", nameof(token));

            if (usuarioId == Guid.Empty)
                throw new ArgumentException("O ID do usuário não pode ser vazio", nameof(usuarioId));

            Id = Guid.NewGuid();
            Jti = jti;
            Token = token;
            UsuarioId = usuarioId;
            DataExpiracao = dataExpiracao;
            DataInvalidacao = DateTime.UtcNow;
        }
    }
}
