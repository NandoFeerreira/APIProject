using APIProject.Application.Usuarios.Comandos.RegistrarUsuario;
using Bogus;
using FluentValidation.TestHelper;
using Xunit;

namespace APIProject.UnitTests.Validators
{
    public class RegistrarUsuarioComandoValidadorTests
    {
        private readonly RegistrarUsuarioComandoValidador _validador;
        private readonly Faker _faker;

        public RegistrarUsuarioComandoValidadorTests()
        {
            _validador = new RegistrarUsuarioComandoValidador();
            _faker = new Faker();
        }

        [Fact]
        public void DevePassarQuandoDadosSaoValidos()
        {
            // Arrange
            var comando = new RegistrarUsuarioComando
            {
                Nome = _faker.Name.FullName(),
                Email = _faker.Internet.Email(),
                Senha = "Senha@123",
                ConfirmacaoSenha = "Senha@123"
            };

            // Act & Assert
            var resultado = _validador.TestValidate(comando);
            resultado.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void DeveFalharQuandoNomeEstaVazio()
        {
            // Arrange
            var comando = new RegistrarUsuarioComando
            {
                Nome = string.Empty,
                Email = _faker.Internet.Email(),
                Senha = "Senha@123",
                ConfirmacaoSenha = "Senha@123"
            };

            // Act & Assert
            var resultado = _validador.TestValidate(comando);
            resultado.ShouldHaveValidationErrorFor(x => x.Nome)
                     .WithErrorMessage("Nome é obrigatório");
        }

        [Fact]
        public void DeveFalharQuandoNomeExcedeTamanhoMaximo()
        {
            // Arrange
            var comando = new RegistrarUsuarioComando
            {
                Nome = new string('a', 101),
                Email = _faker.Internet.Email(),
                Senha = "Senha@123",
                ConfirmacaoSenha = "Senha@123"
            };

            // Act & Assert
            var resultado = _validador.TestValidate(comando);
            resultado.ShouldHaveValidationErrorFor(x => x.Nome)
                     .WithErrorMessage("Nome não pode ter mais de 100 caracteres");
        }

        [Fact]
        public void DeveFalharQuandoEmailEstaVazio()
        {
            // Arrange
            var comando = new RegistrarUsuarioComando
            {
                Nome = _faker.Name.FullName(),
                Email = string.Empty,
                Senha = "Senha@123",
                ConfirmacaoSenha = "Senha@123"
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
            var comando = new RegistrarUsuarioComando
            {
                Nome = _faker.Name.FullName(),
                Email = "emailinvalido",
                Senha = "Senha@123",
                ConfirmacaoSenha = "Senha@123"
            };

            // Act & Assert
            var resultado = _validador.TestValidate(comando);
            resultado.ShouldHaveValidationErrorFor(x => x.Email)
                     .WithErrorMessage("Email inválido");
        }

        [Fact]
        public void DeveFalharQuandoEmailExcedeTamanhoMaximo()
        {
            // Arrange
            var comando = new RegistrarUsuarioComando
            {
                Nome = _faker.Name.FullName(),
                Email = $"{new string('a', 90)}@teste.com",
                Senha = "Senha@123",
                ConfirmacaoSenha = "Senha@123"
            };

            // Act & Assert
            var resultado = _validador.TestValidate(comando);
            resultado.ShouldHaveValidationErrorFor(x => x.Email)
                     .WithErrorMessage("Email não pode ter mais de 100 caracteres");
        }

        [Fact]
        public void DeveFalharQuandoSenhaEstaVazia()
        {
            // Arrange
            var comando = new RegistrarUsuarioComando
            {
                Nome = _faker.Name.FullName(),
                Email = _faker.Internet.Email(),
                Senha = string.Empty,
                ConfirmacaoSenha = string.Empty
            };

            // Act & Assert
            var resultado = _validador.TestValidate(comando);
            resultado.ShouldHaveValidationErrorFor(x => x.Senha)
                     .WithErrorMessage("Senha é obrigatória");
        }

        [Fact]
        public void DeveFalharQuandoSenhasNaoConferem()
        {
            // Arrange
            var comando = new RegistrarUsuarioComando
            {
                Nome = _faker.Name.FullName(),
                Email = _faker.Internet.Email(),
                Senha = "Senha@123",
                ConfirmacaoSenha = "Senha@456"
            };

            // Act & Assert
            var resultado = _validador.TestValidate(comando);
            resultado.ShouldHaveValidationErrorFor(x => x.ConfirmacaoSenha)
                     .WithErrorMessage("As senhas não conferem");
        }
    }
}