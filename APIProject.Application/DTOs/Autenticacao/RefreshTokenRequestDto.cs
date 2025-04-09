﻿namespace APIProject.Application.DTOs.Autenticacao
{
    /// <summary>
    /// DTO para requisição de atualização de token
    /// </summary>
    public class RefreshTokenRequestDto
    {
        /// <summary>
        /// Token JWT expirado
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Token de atualização
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;
    }
}
