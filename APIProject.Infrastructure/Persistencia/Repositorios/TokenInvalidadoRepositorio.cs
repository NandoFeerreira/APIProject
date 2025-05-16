using APIProject.Application.Interfaces;
using APIProject.Domain.Entidades;
using APIProject.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APIProject.Infrastructure.Persistencia.Repositorios
{
    public class TokenInvalidadoRepositorio : ITokenInvalidadoRepositorio
    {
        private readonly DbSet<TokenInvalidado> _tokensInvalidados;
        private readonly ICacheService _cacheService;

        public TokenInvalidadoRepositorio(ApplicationDbContext context, ICacheService cacheService)
        {
            _tokensInvalidados = context.TokensInvalidados;
            _cacheService = cacheService;
        }

        public async Task<bool> TokenEstaInvalidadoAsync(string jti)
        {
            // Primeiro verificamos no cache Redis
            string cacheKey = $"token:invalidated:{jti}";
            bool? cacheResult = await _cacheService.GetAsync<bool>(cacheKey);

            if (cacheResult.HasValue && cacheResult.Value)
            {
                return true;
            }

            // Se não estiver no cache, verificamos no banco de dados
            bool tokenInvalidado = await _tokensInvalidados.AnyAsync(t => t.Jti == jti);

            // Se estiver invalidado, atualizamos o cache
            if (tokenInvalidado)
            {
                var token = await _tokensInvalidados.FirstOrDefaultAsync(t => t.Jti == jti);
                if (token != null)
                {
                    // Armazenamos no cache até a data de expiração do token
                    TimeSpan cacheTime = token.DataExpiracao - DateTime.UtcNow;
                    if (cacheTime.TotalMinutes > 0)
                    {
                        await _cacheService.SetAsync(cacheKey, true, cacheTime);
                    }
                }
            }

            return tokenInvalidado;
        }

        public async Task AdicionarTokenInvalidadoAsync(TokenInvalidado tokenInvalidado)
        {
            // Adicionamos no banco de dados
            await _tokensInvalidados.AddAsync(tokenInvalidado);

            // Adicionamos também no cache para acesso rápido
            string cacheKey = $"token:invalidated:{tokenInvalidado.Jti}";
            
            // Armazenamos no cache até a data de expiração do token
            TimeSpan cacheTime = tokenInvalidado.DataExpiracao - DateTime.UtcNow;
            if (cacheTime.TotalMinutes > 0)
            {
                await _cacheService.SetAsync(cacheKey, true, cacheTime);
                
                // Adicionamos o JTI à lista de todos os tokens invalidados para operações em massa
                var allTokens = await _cacheService.GetAsync<HashSet<string>>("token:invalidated:all") 
                    ?? new HashSet<string>();
                
                allTokens.Add(tokenInvalidado.Jti);
                await _cacheService.SetAsync("token:invalidated:all", allTokens, TimeSpan.FromDays(30));
            }
        }

        public async Task RemoverTokensExpiradosAsync(Guid usuarioId)
        {
            // Removemos do banco de dados
            var tokensExpirados = await _tokensInvalidados
                .Where(t => t.UsuarioId == usuarioId && t.DataExpiracao < DateTime.UtcNow)
                .ToListAsync();

            if (tokensExpirados.Any())
            {
                // Removemos do cache
                foreach (var token in tokensExpirados)
                {
                    string cacheKey = $"token:invalidated:{token.Jti}";
                    await _cacheService.RemoveAsync(cacheKey);
                    
                    // Removemos da lista de tokens invalidados
                    var allTokens = await _cacheService.GetAsync<HashSet<string>>("token:invalidated:all");
                    if (allTokens != null && allTokens.Contains(token.Jti))
                    {
                        allTokens.Remove(token.Jti);
                        await _cacheService.SetAsync("token:invalidated:all", allTokens, TimeSpan.FromDays(30));
                    }
                }
                
                // Removemos do banco de dados
                _tokensInvalidados.RemoveRange(tokensExpirados);
            }
        }

        public async Task RemoverTodosTokensExpiradosAsync()
        {
            // Removemos do banco de dados
            var tokensExpirados = await _tokensInvalidados
                .Where(t => t.DataExpiracao < DateTime.UtcNow)
                .ToListAsync();

            if (tokensExpirados.Any())
            {
                // Removemos do cache
                foreach (var token in tokensExpirados)
                {
                    string cacheKey = $"token:invalidated:{token.Jti}";
                    await _cacheService.RemoveAsync(cacheKey);
                    
                    // Removemos da lista de tokens invalidados
                    var allTokens = await _cacheService.GetAsync<HashSet<string>>("token:invalidated:all");
                    if (allTokens != null && allTokens.Contains(token.Jti))
                    {
                        allTokens.Remove(token.Jti);
                        await _cacheService.SetAsync("token:invalidated:all", allTokens, TimeSpan.FromDays(30));
                    }
                }
                
                // Removemos do banco de dados
                _tokensInvalidados.RemoveRange(tokensExpirados);
            }
        }
    }
}
