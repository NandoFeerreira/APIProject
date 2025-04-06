using APIProject.Application.DTOs;
using APIProject.Application.Usuarios.Comandos.LoginUsuario;
using APIProject.Application.Usuarios.Comandos.RefreshToken;
using APIProject.Application.Usuarios.Comandos.RegistrarUsuario;
using APIProject.Domain.Excecoes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<ActionResult<TokenDto>> RefreshToken([FromBody] RefreshTokenComando comando)
        {
            try
            {
                var token = await _mediator.Send(comando);
                return Ok(token);
            }
            catch (OperacaoNaoAutorizadaException ex)
            {
                return Unauthorized(new { mensagem = ex.Message });
            }
            catch (ValidacaoException ex)
            {
                return BadRequest(new { erros = ex.Erros });
            }
        }


    }
    
}
