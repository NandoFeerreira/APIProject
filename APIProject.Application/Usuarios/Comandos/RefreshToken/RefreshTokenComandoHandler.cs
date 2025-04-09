
using APIProject.Application.Common;
using APIProject.Application.DTOs;
using APIProject.Application.Interfaces;
using APIProject.Domain.Excecoes;
using APIProject.Domain.Interfaces;
using FluentValidation;
using MediatR;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace APIProject.Application.Usuarios.Comandos.RefreshToken
{
    public class RefreshTokenComandoHandler : BaseCommandHandler<RefreshTokenComando, IValidator<RefreshTokenComando>>,
                                             IRequestHandler<RefreshTokenComando, TokenDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;

        public RefreshTokenComandoHandler(
            IUnitOfWork unitOfWork,
            ITokenService tokenService,
            IValidator<RefreshTokenComando> validator) : base(validator)
        {
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
        }

        public async Task<TokenDto> Handle(RefreshTokenComando request, CancellationToken cancellationToken)
        {
            // Validar o comando usando o método da classe base
            await ValidateCommand(request, cancellationToken);

            var principal = _tokenService.ObterPrincipalDeTokenExpirado(request.Token);
            if (principal == null)
            {
                throw new OperacaoNaoAutorizadaException("Token inválido");
            }

            var userId = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ??
                         principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var usuarioId))
            {
                throw new OperacaoNaoAutorizadaException("Token inválido");
            }

            var usuario = await _unitOfWork.Usuarios.ObterPorIdComRefreshTokensAsync(usuarioId);
            if (usuario == null)
            {
                throw new OperacaoNaoAutorizadaException("Usuário não encontrado");
            }

            var refreshToken = usuario.RefreshTokens
                .SingleOrDefault(rt => rt.Token == request.RefreshToken && rt.EstaAtivo);

            if (refreshToken == null)
            {
                throw new OperacaoNaoAutorizadaException("Refresh token inválido ou expirado");
            }

            refreshToken.Utilizado = true;

            foreach (var token in usuario.RefreshTokens.Where(rt => rt.EstaAtivo && rt.Id != refreshToken.Id))
            {
                token.Invalidado = true;
            }


            var novoToken = _tokenService.GerarToken(usuario);

            usuario.AdicionarRefreshToken(
                novoToken.RefreshToken,
                DateTime.UtcNow.AddDays(1)
            );

            await _unitOfWork.CommitAsync();

            return novoToken;
        }
    }
}
