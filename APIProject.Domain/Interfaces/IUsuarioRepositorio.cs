using APIProject.Domain.Entidades;

namespace APIProject.Domain.Interfaces
{
    public interface IUsuarioRepositorio
    {
        Task<Usuario?> ObterPorIdAsync(Guid id);
        Task<IEnumerable<Usuario>> ObterTodosAsync();
        Task<IEnumerable<Usuario>> ObterAtivosAsync();
        Task<Usuario?> ObterPorEmailAsync(string email);
        Task<bool> EmailExisteAsync(string email);
        Task<IEnumerable<Usuario>> BuscarAsync(string termo);
        Task<(IEnumerable<Usuario> Usuarios, int Total)> ObterPaginadoAsync(
            int pagina,
            int tamanhoPagina,
            string? termoBusca = null,
            bool? somenteAtivos = null);
        Task AdicionarAsync(Usuario usuario);
        void Atualizar(Usuario usuario);
        void Remover(Usuario usuario);
       
        Task<Usuario?> ObterPorIdComRefreshTokensAsync(Guid id);

    }
}
