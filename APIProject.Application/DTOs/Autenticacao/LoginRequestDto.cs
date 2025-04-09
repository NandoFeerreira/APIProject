﻿namespace APIProject.Application.DTOs.Autenticacao
{
    /// <summary>
    /// DTO para requisição de login
    /// </summary>
    public class LoginRequestDto
    {
        /// <summary>
        /// Email do usuário
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Senha do usuário
        /// </summary>
        public string Senha { get; set; } = string.Empty;
    }
}
