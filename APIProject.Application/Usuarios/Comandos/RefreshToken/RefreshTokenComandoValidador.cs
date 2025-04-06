
using FluentValidation;

namespace APIProject.Application.Usuarios.Comandos.RefreshToken
{
    public class RefreshTokenComandoValidador : AbstractValidator<RefreshTokenComando>
    {
        public RefreshTokenComandoValidador()
        {
            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("O token é obrigatório");

            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("O refresh token é obrigatório");
        }
    }
}
