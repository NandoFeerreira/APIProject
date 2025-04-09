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

        public bool EstaExpirado => DateTime.UtcNow >= DataExpiracao;
        public bool EstaAtivo => !Utilizado && !Invalidado && !EstaExpirado;

        public RefreshToken()
        {
            DataCriacao = DateTime.UtcNow;
        }

        public RefreshToken(string token, Guid usuarioId, DateTime dataExpiracao)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("O token não pode ser vazio", nameof(token));

            if (usuarioId == Guid.Empty)
                throw new ArgumentException("O ID do usuário não pode ser vazio", nameof(usuarioId));

            if (dataExpiracao <= DateTime.UtcNow)
                throw new ArgumentException("A data de expiração deve ser futura", nameof(dataExpiracao));

            Id = Guid.NewGuid();
            Token = token;
            UsuarioId = usuarioId;
            DataExpiracao = dataExpiracao;
            DataCriacao = DateTime.UtcNow;
            Utilizado = false;
            Invalidado = false;
        }

        public void MarcarComoUtilizado()
        {
            Utilizado = true;
        }

        public void Invalidar()
        {
            Invalidado = true;
        }
    }
}
