using APIProject.Application.DTOs;
using APIProject.Application.Interfaces;
using APIProject.Domain.Excecoes;
using APIProject.Domain.Interfaces;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
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
        private readonly ICacheService _cacheService;
        private const string REFRESH_TOKEN_KEY = "token:refresh:";
        private const string TOKEN_PRINCIPAL_KEY = "token:principal:";

        public RefreshTokenComandoHandler(
            IUnitOfWork unitOfWork,
            ITokenService tokenService,
            IValidator<RefreshTokenComando> validator,
            ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _validator = validator;
            _cacheService = cacheService;
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
            
            // Verificar no cache se o token já foi validado antes
            string principalCacheKey = $"{TOKEN_PRINCIPAL_KEY}{request.Token}";
            var cachedPrincipal = await _cacheService.GetAsync<ClaimsPrincipal>(principalCacheKey);
            ClaimsPrincipal? principal;
            
            if (cachedPrincipal != null)
            {
                principal = cachedPrincipal;
            }
            else
            {
                principal = _tokenService.ObterPrincipalDeTokenExpirado(request.Token);
                
                // Se conseguimos obter o principal, vamos armazenar no cache
                if (principal != null)
                {
                    await _cacheService.SetAsync(principalCacheKey, principal, TimeSpan.FromMinutes(15));
                }
            }
            
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
            
            // Verificamos se o refresh token está no cache primeiro
            string refreshTokenCacheKey = $"{REFRESH_TOKEN_KEY}{usuarioId}:{request.RefreshToken}";
            bool? isTokenValid = await _cacheService.GetAsync<bool>(refreshTokenCacheKey);
            
            // Se já sabemos que o token é inválido, podemos retornar imediatamente
            if (isTokenValid.HasValue && !isTokenValid.Value)
            {
                throw new OperacaoNaoAutorizadaException("Refresh token inválido ou expirado");
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
                // Marcamos no cache que este token é inválido para evitar consultas futuras
                await _cacheService.SetAsync(refreshTokenCacheKey, false, TimeSpan.FromDays(1));
                throw new OperacaoNaoAutorizadaException("Refresh token inválido ou expirado");
            }

            refreshToken.Utilizado = true;

            foreach (var token in usuario.RefreshTokens.Where(rt => rt.EstaAtivo && rt.Id != refreshToken.Id))
            {
                token.Invalidado = true;
                
                // Invalidamos todos os outros tokens no cache
                string otherTokenKey = $"{REFRESH_TOKEN_KEY}{usuarioId}:{token.Token}";
                await _cacheService.SetAsync(otherTokenKey, false, TimeSpan.FromDays(1));
            }
            
            // Invalidamos também o token atual no cache
            await _cacheService.SetAsync(refreshTokenCacheKey, false, TimeSpan.FromDays(1));

            var novoToken = _tokenService.GerarToken(usuario);
         
            usuario.AdicionarRefreshToken(
                novoToken.RefreshToken,
                DateTime.UtcNow.AddDays(1)
            );
            
            // Adicionamos o novo token no cache
            string newTokenKey = $"{REFRESH_TOKEN_KEY}{usuarioId}:{novoToken.RefreshToken}";
            await _cacheService.SetAsync(newTokenKey, true, TimeSpan.FromDays(1));
            
            await _unitOfWork.CommitAsync();

            return novoToken;
        }
    }
}
