using APIProject.Domain.Entidades;
using APIProject.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace APIProject.Infrastructure.Persistencia.Repositorios
{
    public class UsuarioRepositorio : RepositorioBase<Usuario>, IUsuarioRepositorio
    {
        public UsuarioRepositorio(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Usuario> ObterPorEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> EmailExisteAsync(string email)
        {
            return await _dbSet.AnyAsync(u => u.Email == email);
        }
    }
}