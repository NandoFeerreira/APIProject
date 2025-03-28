using APIProject.Application.Usuarios.Comandos.RegistrarUsuario;
using FluentValidation.TestHelper;
using Xunit;

namespace APIProject.UnitTests.Application.Usuarios.Comandos.RegistrarUsuario
{
    public class RegistrarUsuarioComandoValidadorTests
    {
        private readonly RegistrarUsuarioComandoValidador _validator;

        public RegistrarUsuarioComandoValidadorTests()
        {
            _validator = new RegistrarUsuarioComandoValidador();
        }

        [Fact]
        public void Validator_ComDadosValidos_NaoDeveGerarErros()
        {
            // Arrange
            var comando = new RegistrarUsuarioComando
            {
                Nome = "Novo Usuário",
                Email = "usuario@teste.com",
                Senha = "Senha123!",
                ConfirmacaoSenha = "Senha123!"
            };

            // Act
            var result = _validator.TestValidate(comando);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Validator_ComNomeVazioOuNulo_DeveGerarErro(string nome)
        {
            // Arrange
            var comando = new RegistrarUsuarioComando
            {
                Nome = nome,
                Email = "usuario@teste.com",
                Senha = "Senha123!",
                ConfirmacaoSenha = "Senha123!"
            };

            // Act
            var result = _validator.TestValidate(comando);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Nome);
        }

        [Fact]
        public void Validator_ComNomeMuitoLongo_DeveGerarErro()
        {
            // Arrange
            var comando = new RegistrarUsuarioComando
            {
                Nome = new string('A', 101), // 101 caracteres
                Email = "usuario@teste.com",
                Senha = "Senha123!",
                ConfirmacaoSenha = "Senha123!"
            };

            // Act
            var result = _validator.TestValidate(comando);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Nome);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Validator_ComEmailVazioOuNulo_DeveGerarErro(string email)
        {
            // Arrange
            var comando = new RegistrarUsuarioComando
            {
                Nome = "Novo Usuário",
                Email = email,
                Senha = "Senha123!",
                ConfirmacaoSenha = "Senha123!"
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
            var comando = new RegistrarUsuarioComando
            {
                Nome = "Novo Usuário",
                Email = "email_invalido",
                Senha = "Senha123!",
                ConfirmacaoSenha = "Senha123!"
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
            var comando = new RegistrarUsuarioComando
            {
                Nome = "Novo Usuário",
                Email = "usuario@teste.com",
                Senha = senha,
                ConfirmacaoSenha = senha
            };

            // Act
            var result = _validator.TestValidate(comando);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Senha);
        }

        [Fact]
        public void Validator_ComSenhaMuitoCurta_DeveGerarErro()
        {
            // Arrange
            var comando = new RegistrarUsuarioComando
            {
                Nome = "Novo Usuário",
                Email = "usuario@teste.com",
                Senha = "Abc1!",
                ConfirmacaoSenha = "Abc1!"
            };

            // Act
            var result = _validator.TestValidate(comando);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Senha);
        }

        [Fact]
        public void Validator_ComSenhasNaoCorrespondentes_DeveGerarErro()
        {
            // Arrange
            var comando = new RegistrarUsuarioComando
            {
                Nome = "Novo Usuário",
                Email = "usuario@teste.com",
                Senha = "Senha123!",
                ConfirmacaoSenha = "Senha456!"
            };

            // Act
            var result = _validator.TestValidate(comando);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ConfirmacaoSenha);
        }

        [Fact]
        public void Validator_ComSenhaSemLetraMaiuscula_DeveGerarErro()
        {
            // Arrange
            var comando = new RegistrarUsuarioComando
            {
                Nome = "Novo Usuário",
                Email = "usuario@teste.com",
                Senha = "senha123!",
                ConfirmacaoSenha = "senha123!"
            };

            // Act
            var result = _validator.TestValidate(comando);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Senha);
        }

        [Fact]
        public void Validator_ComSenhaSemLetraMinuscula_DeveGerarErro()
        {
            // Arrange
            var comando = new RegistrarUsuarioComando
            {
                Nome = "Novo Usuário",
                Email = "usuario@teste.com",
                Senha = "SENHA123!",
                ConfirmacaoSenha = "SENHA123!"
            };

            // Act
            var result = _validator.TestValidate(comando);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Senha);
        }

        [Fact]
        public void Validator_ComSenhaSemNumero_DeveGerarErro()
        {
            // Arrange
            var comando = new RegistrarUsuarioComando
            {
                Nome = "Novo Usuário",
                Email = "usuario@teste.com",
                Senha = "Senha!!!",
                ConfirmacaoSenha = "Senha!!!"
            };

            // Act
            var result = _validator.TestValidate(comando);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Senha);
        }

        [Fact]
        public void Validator_ComSenhaSemCaractereEspecial_DeveGerarErro()
        {
            // Arrange
            var comando = new RegistrarUsuarioComando
            {
                Nome = "Novo Usuário",
                Email = "usuario@teste.com",
                Senha = "Senha123",
                ConfirmacaoSenha = "Senha123"
            };

            // Act
            var result = _validator.TestValidate(comando);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Senha);
        }
    }
}
