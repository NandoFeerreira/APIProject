using FluentValidation;

namespace APIProject.Application.Usuarios.Comandos.LoginUsuario
{
    public class LoginUsuarioComandoValidador : AbstractValidator<LoginUsuarioComando>
    {
        public LoginUsuarioComandoValidador()
        {
            RuleFor(v => v.Email)
                .NotEmpty().WithMessage("Email é obrigatório")
                .EmailAddress().WithMessage("Email inválido");

            RuleFor(v => v.Senha)
                .NotEmpty().WithMessage("Senha é obrigatória");
        }
    }
}