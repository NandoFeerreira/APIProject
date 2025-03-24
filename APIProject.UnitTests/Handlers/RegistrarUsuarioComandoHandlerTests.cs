using APIProject.Application.DTOs;
using APIProject.Application.Interfaces;
using APIProject.Application.Usuarios.Comandos.RegistrarUsuario;
using APIProject.Domain.Entidades;
using APIProject.Domain.Interfaces;
using AutoMapper;
using Bogus;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace APIProject.UnitTests.Handlers
{
    public class RegistrarUsuarioComandoHandlerTests
    {
        private readonly Mock<IUsuarioRepositorio> _usuarioRepositorioMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IHashService> _hashServiceMock;
        private readonly RegistrarUsuarioComandoHandler _handler;
        private readonly Faker _faker;

        public RegistrarUsuarioComandoHandlerTests()
        {
            _usuarioRepositorioMock = new Mock<IUsuarioRepositorio>();
            _mapperMock = new Mock<IMapper>();
            _hashServiceMock = new Mock<IHashService>();
            _handler = new RegistrarUsuarioComandoHandler(
                _usuarioRepositorioMock.Object,
                _mapperMock.Object,
                _hashServiceMock.Object);
            _faker = new Faker();
        }

        [Fact]
        public async Task Handle_ComDadosValidos_RegistraUsuarioComSucesso()
        {
            // Arrange
            var comando = new RegistrarUsuarioComando
            {
                Nome = _faker.Person.FullName,
                Email = _faker.Internet.Email(),
                Senha = _faker.Internet.Password()
            };

            var senhaCriptografada = _faker.Random.AlphaNumeric(10);
            var usuarioEsperado = new Usuario(comando.Nome, comando.Email, senhaCriptografada);
            var usuarioDtoEsperado = new UsuarioDto
            {
                Id = usuarioEsperado.Id,
                Nome = usuarioEsperado.Nome,
                Email = usuarioEsperado.Email
            };

            _usuarioRepositorioMock.Setup(x => x.EmailExisteAsync(comando.Email))
                .ReturnsAsync(false);

            _hashServiceMock.Setup(x => x.CriarHash(comando.Senha))
                .Returns(senhaCriptografada);

            _mapperMock.Setup(x => x.Map<UsuarioDto>(It.IsAny<Usuario>()))
                .Returns(usuarioDtoEsperado);

            // Act
            var resultado = await _handler.Handle(comando, CancellationToken.None);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(usuarioDtoEsperado.Id, resultado.Id);
            Assert.Equal(usuarioDtoEsperado.Nome, resultado.Nome);
            Assert.Equal(usuarioDtoEsperado.Email, resultado.Email);

            _usuarioRepositorioMock.Verify(x => x.EmailExisteAsync(comando.Email), Times.Once);
            _hashServiceMock.Verify(x => x.CriarHash(comando.Senha), Times.Once);
            _usuarioRepositorioMock.Verify(x => x.AdicionarAsync(It.IsAny<Usuario>()), Times.Once);
            _usuarioRepositorioMock.Verify(x => x.SalvarAsync(), Times.Once);
            _mapperMock.Verify(x => x.Map<UsuarioDto>(It.IsAny<Usuario>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ComEmailJaExistente_LancaExcecao()
        {
            // Arrange
            var comando = new RegistrarUsuarioComando
            {
                Nome = _faker.Person.FullName,
                Email = _faker.Internet.Email(),
                Senha = _faker.Internet.Password()
            };

            _usuarioRepositorioMock.Setup(x => x.EmailExisteAsync(comando.Email))
                .ReturnsAsync(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _handler.Handle(comando, CancellationToken.None));

            Assert.Equal("Email já cadastrado", exception.Message);
            _usuarioRepositorioMock.Verify(x => x.EmailExisteAsync(comando.Email), Times.Once);
            _usuarioRepositorioMock.Verify(x => x.AdicionarAsync(It.IsAny<Usuario>()), Times.Never);
            _usuarioRepositorioMock.Verify(x => x.SalvarAsync(), Times.Never);
        }
    }
}