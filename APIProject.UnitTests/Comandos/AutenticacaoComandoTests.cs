using Moq;
using Xunit;
using APIProject.Application.Interfaces;
using APIProject.Application.Usuarios.Comandos.LoginUsuario;
using APIProject.Application.Usuarios.Comandos.RegistrarUsuario;
using APIProject.Domain.Interfaces;
using APIProject.Domain.Interfaces.Servicos;
using APIProject.Application.DTOs;
using APIProject.Domain.Entidades;
using APIProject.Domain.Excecoes;
using System.Threading;
using System.Threading.Tasks;

namespace APIProject.UnitTests.Comandos
{
    public class AutenticacaoComandoTests
    {
        private readonly Mock<IUsuarioRepositorio> _usuarioRepositorioMock = new();
        private readonly Mock<IHashService> _hashServiceMock = new();
        private readonly Mock<ITokenService> _tokenServiceMock = new();
        private readonly Mock<IUsuarioServico> _usuarioServicoMock = new();

        [Fact]
        public async Task Handle_LoginComando_CredenciaisValidas_RetornaToken()
        {
            // Arrange
            var comando = new LoginUsuarioComando { Email = "teste@teste.com", Senha = "senha123" };
            var handler = new LoginUsuarioComandoHandler(
                _usuarioRepositorioMock.Object,
                _hashServiceMock.Object,
                _tokenServiceMock.Object,
                _usuarioServicoMock.Object);

            var usuario = new Usuario("Teste", comando.Email, "senha_hash");
            usuario.Ativo = true; // Garantir que o usuário está ativo

            _usuarioRepositorioMock.Setup(x => x.ObterPorEmailAsync(comando.Email))
                .ReturnsAsync(usuario);
            _hashServiceMock.Setup(x => x.VerificarHash(comando.Senha, usuario.Senha))
                .Returns(true);
            _tokenServiceMock.Setup(x => x.GerarToken(usuario))
                .Returns(new TokenDto { Token = "token_gerado" });

            // Act
            var resultado = await handler.Handle(comando, CancellationToken.None);

            // Assert
            Assert.Equal("token_gerado", resultado.Token);
            _usuarioServicoMock.Verify(x => x.RegistrarLogin(usuario), Times.Once);
            _usuarioRepositorioMock.Verify(x => x.AtualizarAsync(usuario), Times.Once);
            _usuarioRepositorioMock.Verify(x => x.SalvarAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_LoginComando_UsuarioNaoEncontrado_LancaOperacaoNaoAutorizadaException()
        {
            // Arrange
            var comando = new LoginUsuarioComando { Email = "inexistente@teste.com", Senha = "senha" };
            var handler = new LoginUsuarioComandoHandler(
                _usuarioRepositorioMock.Object,
                _hashServiceMock.Object,
                _tokenServiceMock.Object,
                _usuarioServicoMock.Object);

            _usuarioRepositorioMock.Setup(x => x.ObterPorEmailAsync(comando.Email))
                .ReturnsAsync((Usuario)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<OperacaoNaoAutorizadaException>(
                () => handler.Handle(comando, CancellationToken.None));
            Assert.Equal("Usuário ou senha inválidos", exception.Message);
        }

        [Fact]
        public async Task Handle_LoginComando_UsuarioInativo_LancaOperacaoNaoAutorizadaException()
        {
            // Arrange
            var comando = new LoginUsuarioComando { Email = "inativo@teste.com", Senha = "senha123" };
            var handler = new LoginUsuarioComandoHandler(
                _usuarioRepositorioMock.Object,
                _hashServiceMock.Object,
                _tokenServiceMock.Object,
                _usuarioServicoMock.Object);

            var usuario = new Usuario("Inativo", comando.Email, "senha_hash");
            usuario.Ativo = false; // Usuário inativo

            _usuarioRepositorioMock.Setup(x => x.ObterPorEmailAsync(comando.Email))
                .ReturnsAsync(usuario);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<OperacaoNaoAutorizadaException>(
                () => handler.Handle(comando, CancellationToken.None));
            Assert.Equal("Usuário inativo", exception.Message);
        }

        [Fact]
        public async Task Handle_LoginComando_SenhaInvalida_LancaOperacaoNaoAutorizadaException()
        {
            // Arrange
            var comando = new LoginUsuarioComando { Email = "teste@teste.com", Senha = "senha_errada" };
            var handler = new LoginUsuarioComandoHandler(
                _usuarioRepositorioMock.Object,
                _hashServiceMock.Object,
                _tokenServiceMock.Object,
                _usuarioServicoMock.Object);

            var usuario = new Usuario("Teste", comando.Email, "senha_hash");
            usuario.Ativo = true;

            _usuarioRepositorioMock.Setup(x => x.ObterPorEmailAsync(comando.Email))
                .ReturnsAsync(usuario);
            _hashServiceMock.Setup(x => x.VerificarHash(comando.Senha, usuario.Senha))
                .Returns(false); // Senha inválida

            // Act & Assert
            var exception = await Assert.ThrowsAsync<OperacaoNaoAutorizadaException>(
                () => handler.Handle(comando, CancellationToken.None));
            Assert.Equal("Usuário ou senha inválidos", exception.Message);
        }

        [Fact]
        public async Task Handle_RegistroComando_RegistroValido_CriaUsuario()
        {
            // Arrange
            var comando = new RegistrarUsuarioComando
            {
                Nome = "Novo Usuário",
                Email = "novo@teste.com",
                Senha = "Senha123!",
                ConfirmacaoSenha = "Senha123!" // Adicionado confirmação de senha
            };

            var mapperMock = new Mock<AutoMapper.IMapper>();
            var handler = new RegistrarUsuarioComandoHandler(
                _usuarioRepositorioMock.Object,
                mapperMock.Object,
                _hashServiceMock.Object);

            _hashServiceMock.Setup(x => x.CriarHash(comando.Senha))
                .Returns("hash_senha");
            _usuarioRepositorioMock.Setup(x => x.EmailExisteAsync(comando.Email))
                .ReturnsAsync(false);
            mapperMock.Setup(x => x.Map<UsuarioDto>(It.IsAny<Usuario>()))
                .Returns(new UsuarioDto { Email = comando.Email });

            // Act
            var resultado = await handler.Handle(comando, CancellationToken.None);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(comando.Email, resultado.Email);
            _usuarioRepositorioMock.Verify(x => x.AdicionarAsync(It.Is<Usuario>(u =>
                u.Email == comando.Email && u.Senha == "hash_senha")), Times.Once);
        }

        [Fact]
        public async Task Handle_RegistroComando_EmailJaExistente_LancaDadosDuplicadosException()
        {
            // Arrange
            var comando = new RegistrarUsuarioComando
            {
                Nome = "Usuário Existente",
                Email = "existente@teste.com",
                Senha = "Senha123!",
                ConfirmacaoSenha = "Senha123!"
            };

            var mapperMock = new Mock<AutoMapper.IMapper>();
            var handler = new RegistrarUsuarioComandoHandler(
                _usuarioRepositorioMock.Object,
                mapperMock.Object,
                _hashServiceMock.Object);

            _usuarioRepositorioMock.Setup(x => x.EmailExisteAsync(comando.Email))
                .ReturnsAsync(true); // Email já existe

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DadosDuplicadosException>(
                () => handler.Handle(comando, CancellationToken.None));

            Assert.Equal($"Já existe um(a) Usuário com email '{comando.Email}'.", exception.Message);
        }

        [Fact]
        public async Task Handle_RegistroComando_SenhasNaoCorrespondem_LancaValidacaoException()
        {
            // Arrange
            var comando = new RegistrarUsuarioComando
            {
                Nome = "Novo Usuário",
                Email = "novo@teste.com",
                Senha = "Senha123!",
                ConfirmacaoSenha = "SenhaErrada!"
            };

            var mapperMock = new Mock<AutoMapper.IMapper>();
            var handler = new RegistrarUsuarioComandoHandler(
                _usuarioRepositorioMock.Object,
                mapperMock.Object,
                _hashServiceMock.Object);

            _usuarioRepositorioMock.Setup(x => x.EmailExisteAsync(comando.Email))
                .ReturnsAsync(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidacaoException>(
                () => handler.Handle(comando, CancellationToken.None));

            Assert.Contains("ConfirmacaoSenha", exception.Erros.Keys);
            Assert.Contains("A senha e a confirmação de senha não correspondem.", exception.Erros["ConfirmacaoSenha"]);
        }
    }
}

