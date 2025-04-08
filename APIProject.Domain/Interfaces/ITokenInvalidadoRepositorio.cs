using APIProject.Domain.Entidades;

namespace APIProject.Domain.Interfaces
{
    public interface ITokenInvalidadoRepositorio
    {
        Task<bool> TokenEstaInvalidadoAsync(string jti);
        Task AdicionarTokenInvalidadoAsync(TokenInvalidado tokenInvalidado);
        Task RemoverTokensExpiradosAsync(Guid usuarioId);
        Task RemoverTodosTokensExpiradosAsync();
    }
}
