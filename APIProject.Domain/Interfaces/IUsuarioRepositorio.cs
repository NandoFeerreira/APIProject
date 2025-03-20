using APIProject.Domain.Entidades;
using System;
using System.Threading.Tasks;

namespace APIProject.Domain.Interfaces
{
    public interface IUsuarioRepositorio : IRepositorioBase<Usuario>
    {
        Task<Usuario> ObterPorEmailAsync(string email);
        Task<bool> EmailExisteAsync(string email);
    }
}