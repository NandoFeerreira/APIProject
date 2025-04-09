﻿using APIProject.Domain.Interfaces;
using APIProject.Domain.Interfaces.Servicos;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace APIProject.Infrastructure.Servicos
{
    /// <summary>
    /// Implementação do serviço de validação de tokens
    /// </summary>
    public class TokenValidacaoServico : ITokenValidacaoServico
    {
        private readonly IUnitOfWork _unitOfWork;

        public TokenValidacaoServico(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        /// <summary>
        /// Verifica se um token JWT é válido
        /// </summary>
        /// <param name="token">O token JWT a ser validado</param>
        /// <returns>True se o token for válido, False caso contrário</returns>
        public async Task<(bool EstaValido, string? MensagemErro)> ValidarTokenAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return (false, "Token não fornecido");
            }

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();

                if (!tokenHandler.CanReadToken(token))
                {
                    return (false, "Formato de token inválido");
                }

                var jwtToken = tokenHandler.ReadJwtToken(token);
                var jti = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;

                if (string.IsNullOrEmpty(jti))
                {
                    return (false, "Token sem identificador único (JTI)");
                }

                if (await _unitOfWork.TokensInvalidados.TokenEstaInvalidadoAsync(jti))
                {
                    return (false, "Token inválido ou revogado");
                }

                return (true, null);
            }
            catch (Exception ex)
            {
                // Em um ambiente de produção, você pode querer registrar a exceção em um log
                return (false, $"Erro ao validar token: {ex.Message}");
            }
        }
    }
}
