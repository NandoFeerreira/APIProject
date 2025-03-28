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
    public class RegistrarUsuarioComandoHandler : IRequestHandler<RegistrarUsuarioComando, UsuarioDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHashService _hashService;
        private readonly IValidator<RegistrarUsuarioComando> _validator;

        public RegistrarUsuarioComandoHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IHashService hashService,
            IValidator<RegistrarUsuarioComando> validator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _hashService = hashService;
            _validator = validator;
        }

        public async Task<UsuarioDto> Handle(RegistrarUsuarioComando request, CancellationToken cancellationToken)
        {
            // Validar comando usando o validador existente
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var erros = validationResult.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );

                throw new ValidacaoException(erros);
            }

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
