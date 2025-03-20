using FluentValidation;
using System.Text.RegularExpressions;

namespace APIProject.Application.Usuarios.Comandos.RegistrarUsuario
{
    public class RegistrarUsuarioComandoValidador : AbstractValidator<RegistrarUsuarioComando>
    {
        public RegistrarUsuarioComandoValidador()
        {
            RuleFor(v => v.Nome)
                .NotEmpty().WithMessage("Nome é obrigatório")
                .MaximumLength(100).WithMessage("Nome não pode ter mais de 100 caracteres");

            RuleFor(v => v.Email)
                .NotEmpty().WithMessage("Email é obrigatório")
                .EmailAddress().WithMessage("Email inválido")
                .MaximumLength(100).WithMessage("Email não pode ter mais de 100 caracteres");

            RuleFor(v => v.Senha)
                .NotEmpty().WithMessage("Senha é obrigatória")
                .MinimumLength(6).WithMessage("Senha deve ter pelo menos 6 caracteres")
                .Matches("[A-Z]").WithMessage("Senha deve conter pelo menos uma letra maiúscula")
                .Matches("[a-z]").WithMessage("Senha deve conter pelo menos uma letra minúscula")
                .Matches("[0-9]").WithMessage("Senha deve conter pelo menos um número")
                .Matches("[^a-zA-Z0-9]").WithMessage("Senha deve conter pelo menos um caractere especial");

            RuleFor(v => v.ConfirmacaoSenha)
                .Equal(v => v.Senha).WithMessage("Senhas não conferem");
        }
    }
}