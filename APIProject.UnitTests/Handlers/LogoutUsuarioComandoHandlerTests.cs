using APIProject.Application.Interfaces;
using APIProject.Application.Usuarios.Comandos.Logout;
using APIProject.Domain.Entidades;
using APIProject.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace APIProject.UnitTests.Handlers
{
    public class LogoutUsuarioComandoHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<IUsuarioRepositorio> _usuarioRepositorioMock;
        private readonly Mock<ITokenInvalidadoRepositorio> _tokenInvalidadoRepositorioMock;
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly LogoutUsuarioComandoHandler _handler;

        public LogoutUsuarioComandoHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _usuarioRepositorioMock = new Mock<IUsuarioRepositorio>();
            _tokenInvalidadoRepositorioMock = new Mock<ITokenInvalidadoRepositorio>();
            _cacheServiceMock = new Mock<ICacheService>();

            // Configurar o UnitOfWork para retornar os repositórios mockados
            _unitOfWorkMock.Setup(uow => uow.Usuarios).Returns(_usuarioRepositorioMock.Object);
            _unitOfWorkMock.Setup(uow => uow.TokensInvalidados).Returns(_tokenInvalidadoRepositorioMock.Object);

            _handler = new LogoutUsuarioComandoHandler(
                _unitOfWorkMock.Object,
                _httpContextAccessorMock.Object,
                _cacheServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ComUsuarioValido_InvalidaTokens()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();
            var comando = new LogoutUsuarioComando { UsuarioId = usuarioId };

            var usuario = new Usuario("Teste", "teste@teste.com", "senha_hash");
            var refreshToken = new RefreshToken
            {
                Token = "refresh_token",
                DataExpiracao = DateTime.UtcNow.AddDays(1),
                Invalidado = false,
                Utilizado = false
            };
            usuario.RefreshTokens.Add(refreshToken);

            _usuarioRepositorioMock.Setup(x => x.ObterPorIdComRefreshTokensAsync(usuarioId))
                .ReturnsAsync(usuario);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

            // Act
            await _handler.Handle(comando, CancellationToken.None);

            // Assert
            _tokenInvalidadoRepositorioMock.Verify(x => x.AdicionarTokenInvalidadoAsync(It.IsAny<TokenInvalidado>()), Times.Once);
            _tokenInvalidadoRepositorioMock.Verify(x => x.RemoverTokensExpiradosAsync(usuarioId), Times.Once);
            _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
            _cacheServiceMock.Verify(x => x.RemoveAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Handle_SemTokenNoHeader_ApenasInvalidaRefreshTokens()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();
            var comando = new LogoutUsuarioComando { UsuarioId = usuarioId };

            var usuario = new Usuario("Teste", "teste@teste.com", "senha_hash");
            var refreshToken = new RefreshToken
            {
                Token = "refresh_token",
                DataExpiracao = DateTime.UtcNow.AddDays(1),
                Invalidado = false,
                Utilizado = false
            };
            usuario.RefreshTokens.Add(refreshToken);

            _usuarioRepositorioMock.Setup(x => x.ObterPorIdComRefreshTokensAsync(usuarioId))
                .ReturnsAsync(usuario);

            var httpContext = new DefaultHttpContext(); // Sem token no header
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

            // Act
            await _handler.Handle(comando, CancellationToken.None);

            // Assert
            _tokenInvalidadoRepositorioMock.Verify(x => x.AdicionarTokenInvalidadoAsync(It.IsAny<TokenInvalidado>()), Times.Never);
            _tokenInvalidadoRepositorioMock.Verify(x => x.RemoverTokensExpiradosAsync(usuarioId), Times.Once);
            _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
            _cacheServiceMock.Verify(x => x.RemoveAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Handle_UsuarioNaoEncontrado_LancaExcecao()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();
            var comando = new LogoutUsuarioComando { UsuarioId = usuarioId };

            _usuarioRepositorioMock.Setup(x => x.ObterPorIdComRefreshTokensAsync(usuarioId))
                .ReturnsAsync((Usuario)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _handler.Handle(comando, CancellationToken.None));

            Assert.Equal("Usuário não encontrado", exception.Message);
        }
    }
}