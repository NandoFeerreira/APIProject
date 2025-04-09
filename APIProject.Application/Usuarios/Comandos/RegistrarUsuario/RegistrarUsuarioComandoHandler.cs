using APIProject.Application.Common;
using APIProject.Application.DTOs;
using APIProject.Application.Interfaces;
using APIProject.Domain.Entidades;
using APIProject.Domain.Excecoes;
using APIProject.Domain.Interfaces;
using AutoMapper;
using FluentValidation;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace APIProject.Application.Usuarios.Comandos.RegistrarUsuario
{
    public class RegistrarUsuarioComandoHandler : BaseCommandHandler<RegistrarUsuarioComando, IValidator<RegistrarUsuarioComando>>,
                                                IRequestHandler<RegistrarUsuarioComando, UsuarioDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHashService _hashService;

        public RegistrarUsuarioComandoHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IHashService hashService,
            IValidator<RegistrarUsuarioComando> validator) : base(validator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _hashService = hashService;
        }

        public async Task<UsuarioDto> Handle(RegistrarUsuarioComando request, CancellationToken cancellationToken)
        {
            // Validar o comando usando o método da classe base
            await ValidateCommand(request, cancellationToken);

            // Verificar se o email já existe
            if (await _unitOfWork.Usuarios.EmailExisteAsync(request.Email))
            {
                throw new DadosDuplicadosException("Usuário", "email", request.Email);
            }

            // Criar novo usuário
            var usuario = new Usuario(
                request.Nome,
                request.Email,
                _hashService.CriarHash(request.Senha)
            );

            await _unitOfWork.Usuarios.AdicionarAsync(usuario);
            await _unitOfWork.CommitAsync();

            return _mapper.Map<UsuarioDto>(usuario);
        }
    }
}
