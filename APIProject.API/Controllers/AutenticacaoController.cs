using APIProject.Application.DTOs;
using APIProject.Application.Usuarios.Comandos.LoginUsuario;
using APIProject.Application.Usuarios.Comandos.RegistrarUsuario;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace APIProject.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AutenticacaoController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AutenticacaoController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("registrar")]
        [AllowAnonymous]
        public async Task<ActionResult<UsuarioDto>> Registrar(RegistroUsuarioDto registroDto)
        {
            var comando = new RegistrarUsuarioComando
            {
                Nome = registroDto.Nome,
                Email = registroDto.Email,
                Senha = registroDto.Senha,
                ConfirmacaoSenha = registroDto.ConfirmacaoSenha
            };

            var resultado = await _mediator.Send(comando);
            return Ok(resultado);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<TokenDto>> Login(LoginUsuarioDto loginDto)
        {
            var comando = new LoginUsuarioComando
            {
                Email = loginDto.Email,
                Senha = loginDto.Senha
            };

            var resultado = await _mediator.Send(comando);
            return Ok(resultado);
        }
    }
}