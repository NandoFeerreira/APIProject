using APIProject.Application.Usuarios.Comandos.LoginUsuario;
using FluentValidation.TestHelper;
using Xunit;

namespace APIProject.UnitTests.Application.Usuarios.Comandos.LoginUsuario
{
    public class LoginUsuarioComandoValidadorTests
    {
        private readonly LoginUsuarioComandoValidador _validator;

        public LoginUsuarioComandoValidadorTests()
        {
            _validator = new LoginUsuarioComandoValidador();
        }

        [Fact]
        public void Validator_ComEmailESenhaValidos_NaoDeveGerarErros()
        {
            // Arrange
            var comando = new LoginUsuarioComando
            {
                Email = "usuario@teste.com",
                Senha = "senha123"
            };

            // Act
            var result = _validator.TestValidate(comando);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Validator_ComEmailVazioOuNulo_DeveGerarErro(string email)
        {
            // Arrange
            var comando = new LoginUsuarioComando
            {
                Email = email,
                Senha = "senha123"
            };

            // Act
            var result = _validator.TestValidate(comando);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public void Validator_ComEmailInvalido_DeveGerarErro()
        {
            // Arrange
            var comando = new LoginUsuarioComando
            {
                Email = "email_invalido",
                Senha = "senha123"
            };

            // Act
            var result = _validator.TestValidate(comando);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Validator_ComSenhaVaziaOuNula_DeveGerarErro(string senha)
        {
            // Arrange
            var comando = new LoginUsuarioComando
            {
                Email = "usuario@teste.com",
                Senha = senha
            };

            // Act
            var result = _validator.TestValidate(comando);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Senha);
        }
    }
}
