﻿namespace APIProject.Application.DTOs.Autenticacao
{
    /// <summary>
    /// DTO para requisição de registro de usuário
    /// </summary>
    public class RegistroRequestDto
    {
        /// <summary>
        /// Nome do usuário
        /// </summary>
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Email do usuário
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Senha do usuário
        /// </summary>
        public string Senha { get; set; } = string.Empty;

        /// <summary>
        /// Confirmação da senha
        /// </summary>
        public string ConfirmacaoSenha { get; set; } = string.Empty;
    }
}
