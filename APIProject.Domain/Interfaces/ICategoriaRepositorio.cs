using APIProject.Domain.Entidades;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace APIProject.Domain.Interfaces
{
    public interface ICategoriaRepositorio : IRepositorioBase<Categoria>
    {
        Task<IEnumerable<Categoria>> ObterCategoriasComSubcategoriasAsync();
        Task<IEnumerable<Categoria>> ObterSubcategoriasAsync(Guid categoriaPaiId);
    }
}