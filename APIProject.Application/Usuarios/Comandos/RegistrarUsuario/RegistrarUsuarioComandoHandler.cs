using APIProject.Application.DTOs;
using APIProject.Application.Interfaces;
using APIProject.Domain.Entidades;
using APIProject.Domain.Excecoes;
using APIProject.Domain.Interfaces;
using AutoMapper;
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

        public RegistrarUsuarioComandoHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IHashService hashService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _hashService = hashService;
        }

        public async Task<UsuarioDto> Handle(RegistrarUsuarioComando request, CancellationToken cancellationToken)
        {
            // Verificar se o email já existe
            if (await _unitOfWork.Usuarios.EmailExisteAsync(request.Email))
            {
                throw new DadosDuplicadosException("Usuário", "email", request.Email);
            }

            // Validar a confirmação de senha
            if (request.Senha != request.ConfirmacaoSenha)
            {
                throw new ValidacaoException("ConfirmacaoSenha", "A senha e a confirmação de senha não correspondem.");
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

