using APIProject.Domain.Entidades;
using APIProject.Domain.Interfaces.Servicos;
using System;

namespace APIProject.Domain.Servicos
{
    public class UsuarioServico : IUsuarioServico
    {
        public void AtualizarNome(Usuario usuario, string novoNome)
        {
            if (usuario == null)
                throw new ArgumentNullException(nameof(usuario));

            if (string.IsNullOrWhiteSpace(novoNome))
                throw new ArgumentException("Nome não pode ser vazio", nameof(novoNome));

            usuario.Nome = novoNome;
        }

        public void AtualizarEmail(Usuario usuario, string novoEmail)
        {
            if (usuario == null)
                throw new ArgumentNullException(nameof(usuario));

            if (string.IsNullOrWhiteSpace(novoEmail))
                throw new ArgumentException("Email não pode ser vazio", nameof(novoEmail));

            usuario.Email = novoEmail;
        }

        public void AtualizarSenha(Usuario usuario, string novaSenhaCriptografada)
        {
            if (usuario == null)
                throw new ArgumentNullException(nameof(usuario));

            if (string.IsNullOrWhiteSpace(novaSenhaCriptografada))
                throw new ArgumentException("Senha não pode ser vazia", nameof(novaSenhaCriptografada));

            usuario.Senha = novaSenhaCriptografada;
        }

        public void DesativarUsuario(Usuario usuario)
        {
            if (usuario == null)
                throw new ArgumentNullException(nameof(usuario));

            usuario.Ativo = false;
        }

        public void AtivarUsuario(Usuario usuario)
        {
            if (usuario == null)
                throw new ArgumentNullException(nameof(usuario));

            usuario.Ativo = true;
        }

        public void AdicionarPerfil(Usuario usuario, string perfil)
        {
            if (usuario == null)
                throw new ArgumentNullException(nameof(usuario));

            if (string.IsNullOrWhiteSpace(perfil))
                throw new ArgumentException("Perfil não pode ser vazio", nameof(perfil));

            if (!usuario.Perfis.Contains(perfil))
                usuario.Perfis.Add(perfil);
        }

        public void RemoverPerfil(Usuario usuario, string perfil)
        {
            if (usuario == null)
                throw new ArgumentNullException(nameof(usuario));

            if (string.IsNullOrWhiteSpace(perfil))
                throw new ArgumentException("Perfil não pode ser vazio", nameof(perfil));

            if (usuario.Perfis.Count <= 1 && usuario.Perfis.Contains(perfil))
                throw new InvalidOperationException("Usuário deve ter pelo menos um perfil");

            usuario.Perfis.Remove(perfil);
        }

        public void RegistrarLogin(Usuario usuario)
        {
            if (usuario == null)
                throw new ArgumentNullException(nameof(usuario));

            usuario.UltimoLogin = DateTime.UtcNow;
        }
    }
}
