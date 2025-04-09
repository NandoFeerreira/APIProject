﻿using APIProject.Application.DTOs;
using APIProject.Application.DTOs.Autenticacao;
using APIProject.Application.DTOs.Usuarios;
using APIProject.Application.Usuarios.Comandos.LoginUsuario;
using APIProject.Application.Usuarios.Comandos.RefreshToken;
using APIProject.Application.Usuarios.Comandos.RegistrarUsuario;
using APIProject.Domain.Entidades;
using AutoMapper;

namespace APIProject.Application.Mapeamentos
{
    /// <summary>
    /// Perfil de mapeamento do AutoMapper
    /// </summary>
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Mapeamentos de entidades para DTOs
            CreateMap<Usuario, UsuarioResponseDto>();
            CreateMap<Usuario, UsuarioDto>();

            // Mapeamentos entre DTOs
            CreateMap<UsuarioDto, UsuarioResponseDto>();

            // Mapeamentos de DTOs para comandos
            CreateMap<LoginRequestDto, LoginUsuarioComando>();
            CreateMap<RegistroRequestDto, RegistrarUsuarioComando>();
            CreateMap<RefreshTokenRequestDto, RefreshTokenComando>();

            // Mapeamentos de DTOs antigos para novos (para compatibilidade)
            CreateMap<LoginUsuarioDto, LoginRequestDto>();
            CreateMap<RegistroUsuarioDto, RegistroRequestDto>();

            // Mapeamentos de DTOs antigos para comandos (para compatibilidade)
            CreateMap<LoginUsuarioDto, LoginUsuarioComando>();
            CreateMap<RegistroUsuarioDto, RegistrarUsuarioComando>();

            // Mapeamentos de TokenDto para TokenResponseDto
            CreateMap<TokenDto, TokenResponseDto>()
                .ForMember(dest => dest.AccessToken, opt => opt.MapFrom(src => src.Token));
        }
    }
}
