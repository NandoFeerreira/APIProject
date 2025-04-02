using APIProject.Application.DTOs;
using APIProject.Application.Usuarios.Comandos.LoginUsuario;
using APIProject.Application.Usuarios.Comandos.RegistrarUsuario;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace APIProject.API.Controllers
{
    [Route("api/autenticacao")]
    [ApiController]
    public class AutenticacaoController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AutenticacaoController(IMediator mediator)
        {
            _mediator = mediator;
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

            var token = await _mediator.Send(comando);
            return Ok(token);
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

            var usuario = await _mediator.Send(comando);
            return Ok(usuario);
        }

        [HttpGet("verificar-token")]
        [Authorize] 
        public ActionResult<string> VerificarToken()
        {           
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            var userName = User.FindFirst(JwtRegisteredClaimNames.Name)?.Value;

            return Ok(new { mensagem = "Token válido", usuario = userName + userId});
        }
    }
    
}
