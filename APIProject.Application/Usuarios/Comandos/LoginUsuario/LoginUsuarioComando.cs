using APIProject.Application.DTOs;
using MediatR;

namespace APIProject.Application.Usuarios.Comandos.LoginUsuario
{
    public class LoginUsuarioComando : IRequest<TokenDto>
    {
        public string Email { get; set; }
        public string Senha { get; set; }
    }
}