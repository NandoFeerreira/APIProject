namespace APIProject.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUsuarioRepositorio Usuarios { get; }

        // Adicione outros repositórios conforme necessário
        // IOutroRepositorio OutrosRepositorios { get; }

        Task<int> CommitAsync();
    }
}
