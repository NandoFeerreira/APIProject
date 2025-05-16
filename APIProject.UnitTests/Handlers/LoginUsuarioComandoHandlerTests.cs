using APIProject.Application.DTOs;
using APIProject.Application.Interfaces;
using APIProject.Application.Usuarios.Comandos.LoginUsuario;
using APIProject.Domain.Entidades;
using APIProject.Domain.Excecoes;
using APIProject.Domain.Interfaces;
using APIProject.Domain.Interfaces.Servicos;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace APIProject.UnitTests.Handlers
{
    public class LoginUsuarioComandoHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IUsuarioRepositorio> _usuarioRepositorioMock;
        private readonly Mock<IHashService> _hashServiceMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<IUsuarioServico> _usuarioServicoMock;
        private readonly Mock<IValidator<LoginUsuarioComando>> _validatorMock;
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly LoginUsuarioComandoHandler _handler;

        private const string LOGIN_ATTEMPT_KEY_PREFIX = "login:attempt:";
        private const string FAILED_LOGIN_COUNT_KEY_PREFIX = "login:failed:count:";

        public LoginUsuarioComandoHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _usuarioRepositorioMock = new Mock<IUsuarioRepositorio>();
            _hashServiceMock = new Mock<IHashService>();
            _tokenServiceMock = new Mock<ITokenService>();
            _usuarioServicoMock = new Mock<IUsuarioServico>();
            _validatorMock = new Mock<IValidator<LoginUsuarioComando>>();
            _cacheServiceMock = new Mock<ICacheService>();

            // Configurar o UnitOfWork
            _unitOfWorkMock.Setup(uow => uow.Usuarios).Returns(_usuarioRepositorioMock.Object);

            // Configurar o validator
            _validatorMock
                .Setup(x => x.ValidateAsync(It.IsAny<LoginUsuarioComando>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
                
            // Configurar o cache
            _cacheServiceMock
                .Setup(x => x.GetAsync<int?>(It.IsAny<string>()))
                .ReturnsAsync((int?)null);
                
            _cacheServiceMock
                .Setup(x => x.GetAsync<DateTime?>(It.IsAny<string>()))
                .ReturnsAsync((DateTime?)null);

            _handler = new LoginUsuarioComandoHandler(
                _unitOfWorkMock.Object,
                _hashServiceMock.Object,
                _tokenServiceMock.Object,
                _usuarioServicoMock.Object,
                _validatorMock.Object,
                _cacheServiceMock.Object);
        }

        [Fact]
        public async Task Handle_LoginSucesso_RegistraAcessoEResetaContadorFalhas()
        {
            // Arrange
            var comando = new LoginUsuarioComando
            {
                Email = "teste@teste.com",
                Senha = "Senha123!"
            };

            var usuario = new Usuario("Teste", comando.Email, "hash_senha");
            usuario.Ativo = true;

            var tokenDto = new TokenDto
            {
                Token = "jwt_token",
                RefreshToken = "refresh_token",
                Expiracao = DateTime.UtcNow.AddHours(1)
            };

            _usuarioRepositorioMock.Setup(x => x.ObterPorEmailAsync(comando.Email))
                .ReturnsAsync(usuario);

            _hashServiceMock.Setup(x => x.VerificarHash(comando.Senha, usuario.Senha))
                .Returns(true);

            _tokenServiceMock.Setup(x => x.GerarToken(usuario))
                .Returns(tokenDto);

            // Act
            var resultado = await _handler.Handle(comando, CancellationToken.None);

            // Assert
            Assert.Equal(tokenDto, resultado);
            
            // Verifica que o login foi registrado
            _usuarioServicoMock.Verify(x => x.RegistrarLogin(usuario), Times.Once);
            
            // Verifica que o contador de falhas foi resetado
            _cacheServiceMock.Verify(x => x.RemoveAsync(
                It.Is<string>(s => s.StartsWith(FAILED_LOGIN_COUNT_KEY_PREFIX))), 
                Times.Once);
                
            // Verifica que um token foi adicionado ao usuário
            Assert.Contains(usuario.RefreshTokens, t => t.Token == tokenDto.RefreshToken);
            
            // Verifica que as mudanças foram salvas
            _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
        }

        [Fact]
