using APIProject.Domain.Entidades;
using APIProject.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIProject.Infrastructure.Persistencia.Repositorios
{
    public class UsuarioRepositorio(ApplicationDbContext context) : IUsuarioRepositorio
    {
        private readonly DbSet<Usuario> _usuarios = context.Usuarios;

        public async Task<Usuario?> ObterPorIdAsync(Guid id)
        {
            return await _usuarios.FindAsync(id);
        }

        public async Task<IEnumerable<Usuario>> ObterTodosAsync()
        {
            return await _usuarios.ToListAsync();
        }

        public async Task<IEnumerable<Usuario>> ObterAtivosAsync()
        {
            return await _usuarios.Where(u => u.Ativo).ToListAsync();
        }

        public async Task<Usuario?> ObterPorEmailAsync(string email)
        {
            return await _usuarios.
                Include(u => u.RefreshTokens).
                FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> EmailExisteAsync(string email)
        {
            return await _usuarios.AnyAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<Usuario>> BuscarAsync(string termo)
        {
            if (string.IsNullOrWhiteSpace(termo))
                return await ObterTodosAsync();

            return await _usuarios
                .Where(u => u.Nome != null && u.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase) ||
                           u.Email != null && u.Email.Contains(termo, StringComparison.OrdinalIgnoreCase))
                .ToListAsync();
        }

        public async Task<(IEnumerable<Usuario> Usuarios, int Total)> ObterPaginadoAsync(
            int pagina,
            int tamanhoPagina,
            string? termoBusca = null,
            bool? somenteAtivos = null)
        {
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
        }

        public async Task AdicionarAsync(Usuario usuario)
        {
            await _usuarios.AddAsync(usuario);
        }

        public void Atualizar(Usuario usuario)
        {
            // O EF Core rastreia automaticamente as alterações em entidades carregadas
            // Não é necessário chamar nenhum método específico para atualizar
            // As alterações serão aplicadas quando o UnitOfWork chamar SaveChangesAsync
        }

        public async Task<Usuario?> ObterPorIdComRefreshTokensAsync(Guid id)
        {
            return await _usuarios
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public void Remover(Usuario usuario)
        {
            _usuarios.Remove(usuario);
        }
    }
}
