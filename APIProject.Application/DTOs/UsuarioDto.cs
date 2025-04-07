using System;
using System.Collections.Generic;

namespace APIProject.Application.DTOs
{
    public class UsuarioDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; }  = string.Empty;
        public string Email { get; set; }  = string.Empty;
        public DateTime DataCriacao { get; set; }
        public DateTime? UltimoLogin { get; set; }
        public bool Ativo { get; set; }       
    }
}