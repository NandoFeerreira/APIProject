using MediatR;

namespace APIProject.Application.Usuarios.Comandos.Logout
{
    public class LogoutUsuarioComando : IRequest
    {
        public Guid UsuarioId { get; set; }
    }
}
