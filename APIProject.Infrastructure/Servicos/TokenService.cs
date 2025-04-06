// APIProject.Infrastructure/Servicos/TokenService.cs
using APIProject.Application.DTOs;
using APIProject.Application.Interfaces;
using APIProject.Domain.Entidades;
using APIProject.Infrastructure.Configuracoes;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace APIProject.Infrastructure.Servicos
{
    public class TokenService : ITokenService
    {
        private readonly JwtConfiguracoes _jwtConfiguracoes;

        public TokenService(IOptions<JwtConfiguracoes> jwtConfiguracoes)
        {
            _jwtConfiguracoes = jwtConfiguracoes.Value;
        }

        public TokenDto GerarToken(Usuario usuario)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
                new Claim(JwtRegisteredClaimNames.Name, usuario.Nome),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfiguracoes.Chave));
            var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);
            var expiracao = DateTime.UtcNow.AddMinutes(_jwtConfiguracoes.ExpiracaoMinutos);

            var token = new JwtSecurityToken(
                issuer: _jwtConfiguracoes.Emissor,
                audience: _jwtConfiguracoes.Audiencia,
                claims: claims,
                expires: expiracao,
                signingCredentials: credenciais
            );

            var refreshToken = GerarRefreshToken();
            var refreshTokenExpiracao = DateTime.UtcNow.AddDays(2); 

            return new TokenDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = refreshToken,
                Expiracao = expiracao
            };
        }

        public string GerarRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public ClaimsPrincipal? ObterPrincipalDeTokenExpirado(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfiguracoes.Chave)),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

                if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}
