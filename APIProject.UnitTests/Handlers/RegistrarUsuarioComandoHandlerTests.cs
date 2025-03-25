using APIProject.Application.DTOs;
using APIProject.Application.Interfaces;
using APIProject.Application.Usuarios.Comandos.RegistrarUsuario;
using APIProject.Domain.Entidades;
using APIProject.Domain.Excecoes;
using APIProject.Domain.Interfaces;
using AutoMapper;
using Moq;
using Xunit;

namespace APIProject.UnitTests.Application.Usuarios.Comandos.RegistrarUsuario
{
    public class RegistrarUsuarioComandoHandlerTests
    {
        private readonly Mock<IUsuarioRepositorio> _usuarioRepositorioMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IHashService> _hashServiceMock;
        private readonly RegistrarUsuarioComandoHandler _handler;

        public RegistrarUsuarioComandoHandlerTests()
        {
            _usuarioRepositorioMock = new Mock<IUsuarioRepositorio>();
            _mapperMock = new Mock<IMapper>();
            _hashServiceMock = new Mock<IHashService>();

            _handler = new RegistrarUsuarioComandoHandler(
                _usuarioRepositorioMock.Object,
                _mapperMock.Object,
                _hashServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ComDadosValidos_RetornaUsuarioCriado()
        {
            // Arrange
            var comando = new RegistrarUsuarioComando
            {
                Nome = "Novo Usuário",
                Email = "novo@teste.com",
                Senha = "Senha123!",
                ConfirmacaoSenha = "Senha123!"
            };

            var senhaHash = "senha_hash";
            var usuario = new Usuario(comando.Nome, comando.Email, senhaHash);
            var usuarioDto = new UsuarioDto
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email
            };

            _usuarioRepositorioMock.Setup(x => x.EmailExisteAsync(comando.Email))
                .ReturnsAsync(false);

            _hashServiceMock.Setup(x => x.CriarHash(comando.Senha))
                .Returns(senhaHash);

            _mapperMock.Setup(x => x.Map<UsuarioDto>(It.IsAny<Usuario>()))
                .Returns(usuarioDto);

            // Act
            var result = await _handler.Handle(comando, CancellationToken.None);

            // Assert
            Assert.Equal(usuarioDto, result);
            _usuarioRepositorioMock.Verify(x => x.AdicionarAsync(It.IsAny<Usuario>()), Times.Once);
            _usuarioRepositorioMock.Verify(x => x.SalvarAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ComEmailJaExistente_LancaDadosDuplicadosException()
        {
            // Arrange
            var comando = new RegistrarUsuarioComando
            {
                Nome = "Usuário Existente",
                Email = "existente@teste.com",
                Senha = "Senha123!",
                ConfirmacaoSenha = "Senha123!"
            };

            _usuarioRepositorioMock.Setup(x => x.EmailExisteAsync(comando.Email))
                .ReturnsAsync(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DadosDuplicadosException>(
                () => _handler.Handle(comando, CancellationToken.None));

            Assert.Equal($"Já existe um(a) Usuário com email '{comando.Email}'.", exception.Message);
        }

        [Fact]
        public async Task Handle_ComSenhasNaoCorrespondentes_LancaValidacaoException()
        {
            // Arrange
            var comando = new RegistrarUsuarioComando
            {
                Nome = "Novo Usuário",
                Email = "novo@teste.com",
                Senha = "Senha123!",
                ConfirmacaoSenha = "SenhaDiferente!"
            };

            _usuarioRepositorioMock.Setup(x => x.EmailExisteAsync(comando.Email))
                .ReturnsAsync(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidacaoException>(
                () => _handler.Handle(comando, CancellationToken.None));

            Assert.Contains("ConfirmacaoSenha", exception.Erros.Keys);
            Assert.Contains("A senha e a confirmação de senha não correspondem.", exception.Erros["ConfirmacaoSenha"]);
        }
    }
}
