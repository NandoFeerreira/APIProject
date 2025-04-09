
namespace APIProject.Domain.Entidades
{
    public class Usuario
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; }
        public DateTime? UltimoLogin { get; set; }
        public bool Ativo { get; set; }
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = [];

        protected Usuario() { }

        public Usuario(string nome, string email, string senhaCriptografada)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("O nome não pode ser vazio", nameof(nome));

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("O email não pode ser vazio", nameof(email));

            if (string.IsNullOrWhiteSpace(senhaCriptografada))
                throw new ArgumentException("A senha não pode ser vazia", nameof(senhaCriptografada));

            Id = Guid.NewGuid();
            Nome = nome;
            Email = email;
            Senha = senhaCriptografada;
            DataCriacao = DateTime.UtcNow;
            Ativo = true;
            RefreshTokens = [];
        }

        public void AtualizarNome(string novoNome)
        {
            if (string.IsNullOrWhiteSpace(novoNome))
                throw new ArgumentException("O nome não pode ser vazio", nameof(novoNome));

            Nome = novoNome;
        }

        public void AtualizarEmail(string novoEmail)
        {
            if (string.IsNullOrWhiteSpace(novoEmail))
                throw new ArgumentException("O email não pode ser vazio", nameof(novoEmail));

            Email = novoEmail;
        }

        public void AtualizarSenha(string novaSenhaCriptografada)
        {
            if (string.IsNullOrWhiteSpace(novaSenhaCriptografada))
                throw new ArgumentException("A senha não pode ser vazia", nameof(novaSenhaCriptografada));

            Senha = novaSenhaCriptografada;
        }

        public void Desativar()
        {
            Ativo = false;
        }

        public void Ativar()
        {
            Ativo = true;
        }

        public void RegistrarLogin()
        {
            UltimoLogin = DateTime.UtcNow;
        }

        public void AdicionarRefreshToken(string token, DateTime dataExpiracao)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("O token não pode ser vazio", nameof(token));

            if (dataExpiracao <= DateTime.UtcNow)
                throw new ArgumentException("A data de expiração deve ser futura", nameof(dataExpiracao));

            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = token,
                DataCriacao = DateTime.UtcNow,
                DataExpiracao = dataExpiracao,
                UsuarioId = Id,
                Utilizado = false,
                Invalidado = false
            };

            RefreshTokens.Add(refreshToken);
        }

        public void InvalidarTodosRefreshTokens()
        {
            foreach (var token in RefreshTokens.Where(rt => rt.EstaAtivo))
            {
                token.Invalidar();
            }
        }
    }
}
