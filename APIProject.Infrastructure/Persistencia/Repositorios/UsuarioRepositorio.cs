using APIProject.Application.Interfaces;
using APIProject.Domain.Entidades;
using APIProject.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIProject.Infrastructure.Persistencia.Repositorios
{
    public class UsuarioRepositorio : IUsuarioRepositorio
    {        
        private readonly DbSet<Usuario> _usuarios;
        private readonly ICacheService _cacheService;
        
        // Chaves de cache
        private const string USER_BY_ID_KEY = "user:id:";
        private const string USER_BY_EMAIL_KEY = "user:email:";
        private const string USER_REFRESH_TOKENS_KEY = "user:refreshtokens:";
        private const string USER_EMAIL_EXISTS_KEY = "user:email:exists:";
        private const string USERS_ACTIVE_KEY = "users:active";
        private const string USERS_ALL_KEY = "users:all";
        private const string USERS_SEARCH_KEY = "users:search:";
        private const string USERS_PAGINATED_KEY = "users:paginated:";

        public UsuarioRepositorio(ApplicationDbContext context, ICacheService cacheService)
        {
            _usuarios = context.Usuarios;
            _cacheService = cacheService;
        }

        public async Task<Usuario?> ObterPorIdAsync(Guid id)
        {
            string cacheKey = $"{USER_BY_ID_KEY}{id}";
            
            // Tentamos buscar do cache
            return await _cacheService.GetOrCreateAsync(cacheKey, async () => {
                // Se não estiver em cache, buscamos do banco e armazenamos
                var usuario = await _usuarios.FindAsync(id);
                return usuario;
            }, TimeSpan.FromMinutes(15));
        }

        public async Task<IEnumerable<Usuario>> ObterTodosAsync()
        {
            return await _cacheService.GetOrCreateAsync(USERS_ALL_KEY, async () => {
                return await _usuarios.ToListAsync();
            }, TimeSpan.FromMinutes(10));
        }

        public async Task<IEnumerable<Usuario>> ObterAtivosAsync()
        {
            return await _cacheService.GetOrCreateAsync(USERS_ACTIVE_KEY, async () => {
                return await _usuarios.Where(u => u.Ativo).ToListAsync();
            }, TimeSpan.FromMinutes(10));
        }

        public async Task<Usuario?> ObterPorEmailAsync(string email)
        {
            string cacheKey = $"{USER_BY_EMAIL_KEY}{email.ToLower()}";
            
            return await _cacheService.GetOrCreateAsync(cacheKey, async () => {
                return await _usuarios
                    .Include(u => u.RefreshTokens)
                    .FirstOrDefaultAsync(u => u.Email == email);
            }, TimeSpan.FromMinutes(15));
        }

        public async Task<bool> EmailExisteAsync(string email)
        {
            string cacheKey = $"{USER_EMAIL_EXISTS_KEY}{email.ToLower()}";
            
            return await _cacheService.GetOrCreateAsync(cacheKey, async () => {
                return await _usuarios.AnyAsync(u => u.Email == email);
            }, TimeSpan.FromMinutes(15));
        }

        public async Task<IEnumerable<Usuario>> BuscarAsync(string termo)
        {
            if (string.IsNullOrWhiteSpace(termo))
                return await ObterTodosAsync();

            string cacheKey = $"{USERS_SEARCH_KEY}{termo.ToLower()}";
            
            return await _cacheService.GetOrCreateAsync(cacheKey, async () => {
                return await _usuarios
                    .Where(u => u.Nome != null && u.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase) ||
                               u.Email != null && u.Email.Contains(termo, StringComparison.OrdinalIgnoreCase))
                    .ToListAsync();
            }, TimeSpan.FromMinutes(10));
        }

        public async Task<(IEnumerable<Usuario> Usuarios, int Total)> ObterPaginadoAsync(
            int pagina,
            int tamanhoPagina,
            string? termoBusca = null,
            bool? somenteAtivos = null)
        {
            string termo = termoBusca?.ToLower() ?? "null";
            string ativo = somenteAtivos?.ToString() ?? "null";
            string cacheKey = $"{USERS_PAGINATED_KEY}p{pagina}:t{tamanhoPagina}:q{termo}:a{ativo}";
            
            return await _cacheService.GetOrCreateAsync(cacheKey, async () => {
                IQueryable<Usuario> query = _usuarios;

                if (somenteAtivos.HasValue)
                {
                    query = query.Where(u => u.Ativo == somenteAtivos.Value);
                }

                if (!string.IsNullOrWhiteSpace(termoBusca))
                {
                    termoBusca = termoBusca.ToLower();
                    query = query.Where(u =>
                        EF.Functions.Like(u.Nome!.ToLower(), $"%{termoBusca}%") ||
                        EF.Functions.Like(u.Email!.ToLower(), $"%{termoBusca}%"));
                }
                int total = await query.CountAsync();

                var usuarios = await query
                    .OrderBy(u => u.Nome)
                    .Skip((pagina - 1) * tamanhoPagina)
                    .Take(tamanhoPagina)
                    .ToListAsync();

                return (usuarios, total);
            }, TimeSpan.FromMinutes(5));
        }

        public async Task AdicionarAsync(Usuario usuario)
        {
            await _usuarios.AddAsync(usuario);
            
            // Invalidar caches relevantes
            await InvalidarCachesUsuario(usuario);
            await _cacheService.RemoveAsync(USERS_ALL_KEY);
            await _cacheService.RemoveAsync(USERS_ACTIVE_KEY);
        }

        public void Atualizar(Usuario usuario)
        {
            // Invalidar caches relevantes
            InvalidarCachesUsuario(usuario).GetAwaiter().GetResult();
            _cacheService.RemoveAsync(USERS_ALL_KEY).GetAwaiter().GetResult();
            _cacheService.RemoveAsync(USERS_ACTIVE_KEY).GetAwaiter().GetResult();
        }

        public async Task<Usuario?> ObterPorIdComRefreshTokensAsync(Guid id)
        {
            string cacheKey = $"{USER_REFRESH_TOKENS_KEY}{id}";
            
            return await _cacheService.GetOrCreateAsync(cacheKey, async () => {
                return await _usuarios
                    .Include(u => u.RefreshTokens)
                    .FirstOrDefaultAsync(u => u.Id == id);
            }, TimeSpan.FromMinutes(30));
        }

        public void Remover(Usuario usuario)
        {
            _usuarios.Remove(usuario);
            
            // Invalidar caches relevantes
            InvalidarCachesUsuario(usuario).GetAwaiter().GetResult();
            _cacheService.RemoveAsync(USERS_ALL_KEY).GetAwaiter().GetResult();
            _cacheService.RemoveAsync(USERS_ACTIVE_KEY).GetAwaiter().GetResult();
        }
        
        private async Task InvalidarCachesUsuario(Usuario usuario)
        {
            await _cacheService.RemoveAsync($"{USER_BY_ID_KEY}{usuario.Id}");
            if (!string.IsNullOrEmpty(usuario.Email))
            {
                await _cacheService.RemoveAsync($"{USER_BY_EMAIL_KEY}{usuario.Email.ToLower()}");
                await _cacheService.RemoveAsync($"{USER_EMAIL_EXISTS_KEY}{usuario.Email.ToLower()}");
            }
            await _cacheService.RemoveAsync($"{USER_REFRESH_TOKENS_KEY}{usuario.Id}");
            
            // Invalidar caches de busca e paginação - mais complexo
            // Uma abordagem simplificada é limpar todos os caches desse tipo após um tempo
            // Em uma implementação mais sofisticada, poderíamos usar tags para agrupar caches
        }
    }
}
