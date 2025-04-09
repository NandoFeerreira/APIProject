using APIProject.Application.DTOs;
using APIProject.Application.DTOs.Autenticacao;
using APIProject.Application.DTOs.Usuarios;
using APIProject.Application.Usuarios.Comandos.LoginUsuario;
using APIProject.Application.Usuarios.Comandos.Logout;
using APIProject.Application.Usuarios.Comandos.RefreshToken;
using APIProject.Application.Usuarios.Comandos.RegistrarUsuario;
using APIProject.Domain.Excecoes;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace APIProject.API.Controllers
{
    /// <summary>
    /// Controlador para operações de autenticação
    /// </summary>
    [Route("api/autenticacao")]
    [ApiController]
    [Produces("application/json")]
    public class AutenticacaoController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        /// <summary>
        /// Construtor do controlador de autenticação
        /// </summary>
        /// <param name="mediator">Instância do mediador para envio de comandos</param>
        /// <param name="mapper">Instância do AutoMapper para mapeamento de objetos</param>
        public AutenticacaoController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Realiza o login do usuário
        /// </summary>
        /// <param name="loginDto">Dados de login</param>
        /// <returns>Token de autenticação</returns>
        /// <response code="200">Retorna o token de autenticação</response>
        /// <response code="400">Retorna os erros de validação</response>
        /// <response code="401">Credenciais inválidas</response>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(TokenResponseDto), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 401)]
        public async Task<ActionResult<TokenResponseDto>> Login(LoginRequestDto loginDto)
        {
            try
            {
                var comando = _mapper.Map<LoginUsuarioComando>(loginDto);
                var tokenDto = await _mediator.Send(comando);
                var response = _mapper.Map<TokenResponseDto>(tokenDto);
                return Ok(response);
            }
            catch (ValidacaoException ex)
            {
                return BadRequest(new { erros = ex.Erros });
            }
            catch (OperacaoNaoAutorizadaException ex)
            {
                return Unauthorized(new { mensagem = ex.Message });
            }
        }

        /// <summary>
        /// Registra um novo usuário
        /// </summary>
        /// <param name="registroDto">Dados do usuário</param>
        /// <returns>Dados do usuário criado</returns>
        /// <response code="200">Retorna os dados do usuário criado</response>
        /// <response code="400">Retorna os erros de validação</response>
        /// <response code="409">Retorna erro quando o email já está em uso</response>
        [HttpPost("registrar")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(UsuarioResponseDto), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 409)]
        public async Task<ActionResult<UsuarioResponseDto>> Registrar(RegistroRequestDto registroDto)
        {
            try
            {
                var comando = _mapper.Map<RegistrarUsuarioComando>(registroDto);
                var usuarioDto = await _mediator.Send(comando);
                var response = _mapper.Map<UsuarioResponseDto>(usuarioDto);
                return Ok(response);
            }
            catch (ValidacaoException ex)
            {
                return BadRequest(new { erros = ex.Erros });
            }
            catch (DadosDuplicadosException ex)
            {
                return Conflict(new { mensagem = ex.Message });
            }
        }

        /// <summary>
        /// Atualiza o token de autenticação usando um refresh token
        /// </summary>
        /// <param name="refreshTokenDto">Token e refresh token</param>
        /// <returns>Novo token de autenticação</returns>
        /// <response code="200">Retorna o novo token de autenticação</response>
        /// <response code="400">Retorna os erros de validação</response>
        /// <response code="401">Token ou refresh token inválidos</response>
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(TokenResponseDto), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 401)]
        public async Task<ActionResult<TokenResponseDto>> RefreshToken([FromBody] RefreshTokenRequestDto refreshTokenDto)
        {
            try
            {
                var comando = _mapper.Map<RefreshTokenComando>(refreshTokenDto);
                var tokenDto = await _mediator.Send(comando);
                var response = _mapper.Map<TokenResponseDto>(tokenDto);
                return Ok(response);
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

        /// <summary>
        /// Realiza o logout do usuário, invalidando o token atual
        /// </summary>
        /// <returns>Sem conteúdo</returns>
        /// <response code="204">Logout realizado com sucesso</response>
        /// <response code="401">Usuário não autenticado</response>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ProblemDetails), 401)]
        public async Task<IActionResult> Logout()
        {
            var usuarioId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(usuarioId) || !Guid.TryParse(usuarioId, out var guidUsuarioId))
            {
                return Unauthorized(new { mensagem = "Usuário não autorizado" });
            }

            var comando = new LogoutUsuarioComando
            {
                UsuarioId = guidUsuarioId
            };

            await _mediator.Send(comando);
            return NoContent();
        }
    }
}
