namespace APIProject.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUsuarioRepositorio Usuarios { get; }

        // Adicione outros reposit�rios conforme necess�rio
        // IOutroRepositorio OutrosRepositorios { get; }

        Task<int> CommitAsync();
    }
}
