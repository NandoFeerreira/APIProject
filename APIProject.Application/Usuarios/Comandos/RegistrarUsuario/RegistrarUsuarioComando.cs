using APIProject.Application.DTOs;
using MediatR;

namespace APIProject.Application.Usuarios.Comandos.RegistrarUsuario
{
    public class RegistrarUsuarioComando : IRequest<UsuarioDto>
    {
        public string Nome { get; set; }  = string.Empty;
        public string Email { get; set; }  = string.Empty;
        public string Senha { get; set; }  = string.Empty;
        public string ConfirmacaoSenha { get; set; }  = string.Empty;
    }
}