using APIProject.Domain.Entidades;
using System;
using System.Collections.Generic;

namespace APIProject.Domain.Interfaces.Servicos
{
    public interface IUsuarioServico
    {
        void AtualizarNome(Usuario usuario, string novoNome);
        void AtualizarEmail(Usuario usuario, string novoEmail);
        void AtualizarSenha(Usuario usuario, string novaSenhaCriptografada);
        void DesativarUsuario(Usuario usuario);
        void AtivarUsuario(Usuario usuario);
        void RegistrarLogin(Usuario usuario);
    }
}