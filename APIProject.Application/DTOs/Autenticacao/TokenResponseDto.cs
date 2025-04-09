﻿using System;

namespace APIProject.Application.DTOs.Autenticacao
{
    /// <summary>
    /// DTO para resposta de token de autenticação
    /// </summary>
    public class TokenResponseDto
    {
        /// <summary>
        /// Token JWT de acesso
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// Token de atualização para obter um novo token JWT
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// Data e hora de expiração do token
        /// </summary>
        public DateTime Expiracao { get; set; }

        /// <summary>
        /// Tipo do token
        /// </summary>
        public string TokenType { get; set; } = "Bearer";
    }
}
