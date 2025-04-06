
using APIProject.Application.DTOs;
using MediatR;

namespace APIProject.Application.Usuarios.Comandos.RefreshToken
{
    public class RefreshTokenComando : IRequest<TokenDto>
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
       
    }
}

