using APIProject.Application.Usuarios.Comandos.LoginUsuario;
using Bogus;
using FluentValidation.TestHelper;
using Xunit;

namespace APIProject.UnitTests.Validators
{
    public class LoginUsuarioComandoValidadorTests
    {
        private readonly LoginUsuarioComandoValidador _validador;
        private readonly Faker _faker;

        public LoginUsuarioComandoValidadorTests()
        {
            _validador = new LoginUsuarioComandoValidador();
            _faker = new Faker("pt_BR");
        }

        [Fact]
        public void DevePassarQuandoEmailEhValido()
        {
            // Arrange
            var comando = new LoginUsuarioComando
            {
                Email = _faker.Internet.Email(),
                Senha = _faker.Internet.Password()
            };

            // Act & Assert
            var resultado = _validador.TestValidate(comando);
            resultado.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void DeveFalharQuandoEmailEstaVazio()
        {
            // Arrange
            var comando = new LoginUsuarioComando
            {
                Email = string.Empty,
                Senha = _faker.Internet.Password()
            };

            // Act & Assert
            var resultado = _validador.TestValidate(comando);
            resultado.ShouldHaveValidationErrorFor(x => x.Email)
                     .WithErrorMessage("Email é obrigatório");
        }

        [Fact]
        public void DeveFalharQuandoEmailEhInvalido()
        {
            // Arrange
            var comando = new LoginUsuarioComando
            {
                Email = "emailinvalido",
                Senha = _faker.Internet.Password()
            };

            // Act & Assert
            var resultado = _validador.TestValidate(comando);
            resultado.ShouldHaveValidationErrorFor(x => x.Email)
                     .WithErrorMessage("Email inválido");
        }

        [Fact]
        public void DeveFalharQuandoSenhaEstaVazia()
        {
            // Arrange
            var comando = new LoginUsuarioComando
            {
                Email = _faker.Internet.Email(),
                Senha = string.Empty
            };

            // Act & Assert
            var resultado = _validador.TestValidate(comando);
            resultado.ShouldHaveValidationErrorFor(x => x.Senha)
                     .WithErrorMessage("Senha é obrigatória");
        }
    }
}