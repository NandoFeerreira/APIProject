
using APIProject.Application.DTOs;
using APIProject.Application.Interfaces;
using APIProject.Domain.Excecoes;
using APIProject.Domain.Interfaces;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt; // Adicionar esta importação
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace APIProject.Application.Usuarios.Comandos.RefreshToken
{
    public class RefreshTokenComandoHandler : IRequestHandler<RefreshTokenComando, TokenDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly IValidator<RefreshTokenComando> _validator;

        public RefreshTokenComandoHandler(
            IUnitOfWork unitOfWork,
            ITokenService tokenService,
            IValidator<RefreshTokenComando> validator)
        {
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _validator = validator;
        }

        public async Task<TokenDto> Handle(RefreshTokenComando request, CancellationToken cancellationToken)
        {
            // Validar comando
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var erros = validationResult.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );

                throw new ValidacaoException(erros);
            }
         
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
