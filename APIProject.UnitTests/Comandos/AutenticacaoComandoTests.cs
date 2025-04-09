using APIProject.Application.DTOs;
using APIProject.Application.Interfaces;
using APIProject.Application.Usuarios.Comandos.LoginUsuario;
using APIProject.Application.Usuarios.Comandos.RegistrarUsuario;
using APIProject.Domain.Entidades;
using APIProject.Domain.Excecoes;
using APIProject.Domain.Interfaces;
using APIProject.Domain.Interfaces.Servicos;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Xunit;

namespace APIProject.UnitTests.Comandos
{
    /// <summary>
    /// Fixture compartilhada para os testes de autenticação
    /// </summary>
    public class AutenticacaoFixture
    {
        public Mock<IUnitOfWork> UnitOfWorkMock { get; }
        public Mock<IUsuarioRepositorio> UsuarioRepositorioMock { get; }
        public Mock<IHashService> HashServiceMock { get; }
        public Mock<ITokenService> TokenServiceMock { get; }
        public Mock<IUsuarioServico> UsuarioServicoMock { get; }
        public Mock<IValidator<LoginUsuarioComando>> LoginValidatorMock { get; }
        public Mock<IValidator<RegistrarUsuarioComando>> RegistroValidatorMock { get; }
        public Mock<IMapper> MapperMock { get; }

        public AutenticacaoFixture()
        {
            UnitOfWorkMock = new Mock<IUnitOfWork>();
            UsuarioRepositorioMock = new Mock<IUsuarioRepositorio>();
            HashServiceMock = new Mock<IHashService>();
            TokenServiceMock = new Mock<ITokenService>();
            UsuarioServicoMock = new Mock<IUsuarioServico>();
            LoginValidatorMock = new Mock<IValidator<LoginUsuarioComando>>();
            RegistroValidatorMock = new Mock<IValidator<RegistrarUsuarioComando>>();
            MapperMock = new Mock<IMapper>();

            // Configurar o UnitOfWork para retornar o repositório de usuários mockado
            UnitOfWorkMock.Setup(uow => uow.Usuarios).Returns(UsuarioRepositorioMock.Object);

            // Configurar os validadores para retornar sucesso por padrão
            LoginValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<LoginUsuarioComando>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            RegistroValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<RegistrarUsuarioComando>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
        }

        public LoginUsuarioComandoHandler CreateLoginHandler()
        {
            return new LoginUsuarioComandoHandler(
                UnitOfWorkMock.Object,
                HashServiceMock.Object,
                TokenServiceMock.Object,
                UsuarioServicoMock.Object,
                LoginValidatorMock.Object);
        }

        public RegistrarUsuarioComandoHandler CreateRegistroHandler()
        {
            return new RegistrarUsuarioComandoHandler(
                UnitOfWorkMock.Object,
                MapperMock.Object,
                HashServiceMock.Object,
                RegistroValidatorMock.Object);
        }

        public static Usuario CriarUsuarioTeste(string nome, string email, string senhaHash, bool ativo = true)
        {
            var usuario = new Usuario(nome, email, senhaHash)
            {
                Ativo = ativo
            };
            return usuario;
        }
    }

    public class AutenticacaoComandoTests : IClassFixture<AutenticacaoFixture>
    {
        private readonly AutenticacaoFixture _fixture;

        public AutenticacaoComandoTests(AutenticacaoFixture fixture)
        {
            _fixture = fixture;

            // Resetar as configurações dos mocks antes de cada teste
            _fixture.UsuarioRepositorioMock.Reset();
            _fixture.HashServiceMock.Reset();
            _fixture.TokenServiceMock.Reset();
            _fixture.UsuarioServicoMock.Reset();
            _fixture.LoginValidatorMock.Reset();
            _fixture.RegistroValidatorMock.Reset();
            _fixture.MapperMock.Reset();
            _fixture.UnitOfWorkMock.Reset();

            // Reconfigurar o UnitOfWork após o reset
            _fixture.UnitOfWorkMock.Setup(uow => uow.Usuarios).Returns(_fixture.UsuarioRepositorioMock.Object);

            // Reconfigurar os validadores com sucesso por padrão
            _fixture.LoginValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<LoginUsuarioComando>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _fixture.RegistroValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<RegistrarUsuarioComando>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
        }

        #region Login Tests

        [Fact]
        public async Task Handle_LoginComando_CredenciaisValidas_RetornaToken()
        {
            // Arrange
            var comando = new LoginUsuarioComando { Email = "teste@teste.com", Senha = "senha123" };
            var handler = _fixture.CreateLoginHandler();

            var usuario = AutenticacaoFixture.CriarUsuarioTeste("Teste", comando.Email, "senha_hash");

            _fixture.UsuarioRepositorioMock.Setup(x => x.ObterPorEmailAsync(comando.Email))
                .ReturnsAsync(usuario);
            _fixture.HashServiceMock.Setup(x => x.VerificarHash(comando.Senha, usuario.Senha!))
                .Returns(true);
            _fixture.TokenServiceMock.Setup(x => x.GerarToken(usuario))
                .Returns(new TokenDto { Token = "token_gerado", RefreshToken = "refresh_token" });

            // Act
            var resultado = await handler.Handle(comando, CancellationToken.None);

            // Assert
            Assert.Equal("token_gerado", resultado.Token);
            _fixture.UsuarioServicoMock.Verify(x => x.RegistrarLogin(usuario), Times.Once);
            _fixture.UnitOfWorkMock.Verify(uow => uow.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_LoginComando_ValidacaoInvalida_LancaValidacaoException()
        {
            // Arrange
            var comando = new LoginUsuarioComando { Email = "email_invalido", Senha = "" };
            var handler = _fixture.CreateLoginHandler();

            var validationFailures = new List<ValidationFailure>
            {
                new("Email", "Email inválido"),
                new("Senha", "Senha é obrigatória")
            };

            _fixture.LoginValidatorMock.Setup(v => v.ValidateAsync(comando, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(validationFailures));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidacaoException>(
                () => handler.Handle(comando, CancellationToken.None));

            Assert.Contains("Email", exception.Erros.Keys);
            Assert.Contains("Senha", exception.Erros.Keys);
        }

        [Fact]
        public async Task Handle_LoginComando_UsuarioNaoEncontrado_LancaOperacaoNaoAutorizadaException()
        {
            // Arrange
            var comando = new LoginUsuarioComando { Email = "inexistente@teste.com", Senha = "senha" };
            var handler = _fixture.CreateLoginHandler();

            _fixture.UsuarioRepositorioMock.Setup(x => x.ObterPorEmailAsync(comando.Email))
                .ReturnsAsync((Usuario?)null);

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
            var handler = _fixture.CreateLoginHandler();

            var usuario = AutenticacaoFixture.CriarUsuarioTeste("Inativo", comando.Email, "senha_hash", false);

            _fixture.UsuarioRepositorioMock.Setup(x => x.ObterPorEmailAsync(comando.Email))
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
            var handler = _fixture.CreateLoginHandler();

            var usuario = AutenticacaoFixture.CriarUsuarioTeste("Teste", comando.Email, "senha_hash");

            _fixture.UsuarioRepositorioMock.Setup(x => x.ObterPorEmailAsync(comando.Email))
                .ReturnsAsync(usuario);
            _fixture.HashServiceMock.Setup(x => x.VerificarHash(comando.Senha, usuario.Senha!))
                .Returns(false); // Senha inválida

            // Act & Assert
            var exception = await Assert.ThrowsAsync<OperacaoNaoAutorizadaException>(
                () => handler.Handle(comando, CancellationToken.None));
            Assert.Equal("Usuário ou senha inválidos", exception.Message);
        }

        #endregion

        #region Registro Tests

        [Fact]
        public async Task Handle_RegistroComando_RegistroValido_CriaUsuario()
        {
            // Arrange
            var comando = new RegistrarUsuarioComando
            {
                Nome = "Novo Usuário",
                Email = "novo@teste.com",
                Senha = "Senha123!",
                ConfirmacaoSenha = "Senha123!"
            };

            var handler = _fixture.CreateRegistroHandler();

            _fixture.HashServiceMock.Setup(x => x.CriarHash(comando.Senha))
                .Returns("hash_senha");
            _fixture.UsuarioRepositorioMock.Setup(x => x.EmailExisteAsync(comando.Email))
                .ReturnsAsync(false);
            _fixture.MapperMock.Setup(x => x.Map<UsuarioDto>(It.IsAny<Usuario>()))
                .Returns(new UsuarioDto { Email = comando.Email });

            // Act
            var resultado = await handler.Handle(comando, CancellationToken.None);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(comando.Email, resultado.Email);
            _fixture.UsuarioRepositorioMock.Verify(x => x.AdicionarAsync(It.Is<Usuario>(u =>
                u.Email == comando.Email && u.Senha == "hash_senha")), Times.Once);
            _fixture.UnitOfWorkMock.Verify(uow => uow.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_RegistroComando_ValidacaoInvalida_LancaValidacaoException()
        {
            // Arrange
            var comando = new RegistrarUsuarioComando
            {
                Nome = "",
                Email = "email_invalido",
                Senha = "123",
                ConfirmacaoSenha = "456"
            };

            var handler = _fixture.CreateRegistroHandler();

            var validationFailures = new List<ValidationFailure>
            {
                new("Nome", "Nome é obrigatório"),
                new("Email", "Email inválido"),
                new("Senha", "Senha deve ter pelo menos 6 caracteres"),
                new("ConfirmacaoSenha", "Senhas não conferem")
            };

            _fixture.RegistroValidatorMock.Setup(v => v.ValidateAsync(comando, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(validationFailures));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidacaoException>(
                () => handler.Handle(comando, CancellationToken.None));

            Assert.Contains("Nome", exception.Erros.Keys);
            Assert.Contains("Email", exception.Erros.Keys);
            Assert.Contains("Senha", exception.Erros.Keys);
            Assert.Contains("ConfirmacaoSenha", exception.Erros.Keys);
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

            var handler = _fixture.CreateRegistroHandler();

            _fixture.UsuarioRepositorioMock.Setup(x => x.EmailExisteAsync(comando.Email))
                .ReturnsAsync(true); // Email já existe

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DadosDuplicadosException>(
                () => handler.Handle(comando, CancellationToken.None));

            Assert.Equal($"Já existe um(a) Usuário com email '{comando.Email}'.", exception.Message);
        }

        #endregion
    }
}
