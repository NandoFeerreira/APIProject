﻿using System;

namespace APIProject.Application.DTOs.Usuarios
{
    /// <summary>
    /// DTO para resposta de usuário
    /// </summary>
    public class UsuarioResponseDto
    {
        /// <summary>
        /// Identificador único do usuário
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Nome do usuário
        /// </summary>
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Email do usuário
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Data de criação do usuário
        /// </summary>
        public DateTime DataCriacao { get; set; }

        /// <summary>
        /// Data do último login do usuário
        /// </summary>
        public DateTime? UltimoLogin { get; set; }

        /// <summary>
        /// Indica se o usuário está ativo
        /// </summary>
        public bool Ativo { get; set; }
    }
}
