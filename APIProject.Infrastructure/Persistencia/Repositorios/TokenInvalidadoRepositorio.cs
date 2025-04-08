using APIProject.Domain.Entidades;
using APIProject.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APIProject.Infrastructure.Persistencia.Repositorios
{
    public class TokenInvalidadoRepositorio(ApplicationDbContext context) : ITokenInvalidadoRepositorio
    {
        private readonly DbSet<TokenInvalidado> _tokensInvalidados = context.TokensInvalidados;

        public async Task<bool> TokenEstaInvalidadoAsync(string jti)
        {
            return await _tokensInvalidados.AnyAsync(t => t.Jti == jti);
        }

        public async Task AdicionarTokenInvalidadoAsync(TokenInvalidado tokenInvalidado)
        {
            await _tokensInvalidados.AddAsync(tokenInvalidado);
        }

        public async Task RemoverTokensExpiradosAsync(Guid usuarioId)
        {
            var tokensExpirados = await _tokensInvalidados
                .Where(t => t.UsuarioId == usuarioId && t.DataExpiracao < DateTime.UtcNow)
                .ToListAsync();

            if (tokensExpirados.Any())
            {
                _tokensInvalidados.RemoveRange(tokensExpirados);
            }
        }

        public async Task RemoverTodosTokensExpiradosAsync()
        {
            var tokensExpirados = await _tokensInvalidados
                .Where(t => t.DataExpiracao < DateTime.UtcNow)
                .ToListAsync();

            if (tokensExpirados.Any())
            {
                _tokensInvalidados.RemoveRange(tokensExpirados);
            }
        }
    }
}
