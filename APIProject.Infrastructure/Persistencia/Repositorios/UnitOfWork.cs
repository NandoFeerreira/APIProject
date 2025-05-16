using APIProject.Application.Interfaces;
using APIProject.Domain.Interfaces;

namespace APIProject.Infrastructure.Persistencia.Repositorios
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly ICacheService _cacheService;
        private IUsuarioRepositorio? _usuarioRepositorio;
        private ITokenInvalidadoRepositorio? _tokenInvalidadoRepositorio;
        private bool disposed = false;

        public UnitOfWork(ApplicationDbContext context, ICacheService cacheService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        }

        public IUsuarioRepositorio Usuarios => _usuarioRepositorio ??= new UsuarioRepositorio(_context, _cacheService);

        public ITokenInvalidadoRepositorio TokensInvalidados => _tokenInvalidadoRepositorio ??= new TokenInvalidadoRepositorio(_context, _cacheService);

        public async Task<int> CommitAsync()
        {
            return await _context.SaveChangesAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
