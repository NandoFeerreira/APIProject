namespace APIProject.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUsuarioRepositorio Usuarios { get; }
        ITokenInvalidadoRepositorio TokensInvalidados { get; }

        Task<int> CommitAsync();        
        
    }
}
