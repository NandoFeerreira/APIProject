using APIProject.Domain.Entidades;
using APIProject.Domain.Servicos;
using System;
using Xunit;

namespace APIProject.UnitTests.Domain.Servicos
{
    public class UsuarioServicoTests
    {
        private readonly UsuarioServico _usuarioServico;

        public UsuarioServicoTests()
        {
            _usuarioServico = new UsuarioServico();
        }

        [Fact]
        public void AtualizarNome_DeveAtualizarNomeDoUsuario()
        {
            // Arrange
            var usuario = new Usuario("Nome Antigo", "teste@teste.com", "hash");
            var novoNome = "Nome Novo";

            // Act
            _usuarioServico.AtualizarNome(usuario, novoNome);

            // Assert
            Assert.Equal(novoNome, usuario.Nome);
        }

        [Fact]
        public void AtualizarNome_ComNomeVazio_DeveLancarArgumentException()
        {
            // Arrange
            var usuario = new Usuario("Nome Teste", "teste@teste.com", "hash");

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _usuarioServico.AtualizarNome(usuario, ""));
            Assert.Contains("Nome não pode ser vazio", exception.Message);
        }

        [Fact]
        public void AtualizarEmail_DeveAtualizarEmailDoUsuario()
        {
            // Arrange
            var usuario = new Usuario("Nome Teste", "antigo@teste.com", "hash");
            var novoEmail = "novo@teste.com";

            // Act
            _usuarioServico.AtualizarEmail(usuario, novoEmail);

            // Assert
            Assert.Equal(novoEmail, usuario.Email);
        }

        [Fact]
        public void DesativarUsuario_DeveDesativarUsuario()
        {
            // Arrange
            var usuario = new Usuario("Nome Teste", "teste@teste.com", "hash");
            usuario.Ativo = true;

            // Act
            _usuarioServico.DesativarUsuario(usuario);

            // Assert
            Assert.False(usuario.Ativo);
        }

        [Fact]
        public void AtivarUsuario_DeveAtivarUsuario()
        {
            // Arrange
            var usuario = new Usuario("Nome Teste", "teste@teste.com", "hash");
            usuario.Ativo = false;

            // Act
            _usuarioServico.AtivarUsuario(usuario);

            // Assert
            Assert.True(usuario.Ativo);
        }

        [Fact]
        public void RegistrarLogin_DeveAtualizarUltimoLogin()
        {
            // Arrange
            var usuario = new Usuario("Nome Teste", "teste@teste.com", "hash");
            usuario.UltimoLogin = null;

            // Act
            _usuarioServico.RegistrarLogin(usuario);

            // Assert
            Assert.NotNull(usuario.UltimoLogin);
            Assert.True(usuario.UltimoLogin > DateTime.UtcNow.AddMinutes(-1));
        }

     
    }
}
