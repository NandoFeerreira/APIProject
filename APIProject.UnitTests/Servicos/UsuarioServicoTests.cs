using APIProject.Domain.Entidades;
using APIProject.Domain.Interfaces;
using APIProject.Domain.Servicos;
using Moq;
using System;
using Xunit;

namespace APIProject.UnitTests.Servicos
{
    public class UsuarioServicoTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IUsuarioRepositorio> _usuarioRepositorioMock;
        private readonly UsuarioServico _usuarioServico;

        public UsuarioServicoTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _usuarioRepositorioMock = new Mock<IUsuarioRepositorio>();

            _unitOfWorkMock.Setup(x => x.Usuarios).Returns(_usuarioRepositorioMock.Object);

            _usuarioServico = new UsuarioServico();
        }

        [Fact]
        public void RegistrarLogin_DeveAtualizarUltimoLogin()
        {
            // Arrange
            var usuario = new Usuario("Teste", "teste@teste.com", "senha_hash");
            usuario.UltimoLogin = DateTime.UtcNow.AddMinutes(-5); // Definir um valor anterior
            var dataAnterior = usuario.UltimoLogin;

            // Act
            _usuarioServico.RegistrarLogin(usuario);

            // Assert
            Assert.NotEqual(dataAnterior, usuario.UltimoLogin);
            Assert.True(usuario.UltimoLogin > dataAnterior);
        }

        [Fact]
        public void RegistrarLogin_ComUsuarioNull_DeveLancarArgumentNullException()
        {
            // Act & Assert
            Usuario? usuarioNull = null;
            Assert.Throws<ArgumentNullException>(() => _usuarioServico.RegistrarLogin(usuarioNull!));
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
    }
}
