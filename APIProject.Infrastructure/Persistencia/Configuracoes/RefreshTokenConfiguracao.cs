﻿using APIProject.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APIProject.Infrastructure.Persistencia.Configuracoes
{
    public class RefreshTokenConfiguracao : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.HasKey(rt => rt.Id);

            builder.Property(rt => rt.Token)
                .IsRequired();

            builder.Property(rt => rt.DataCriacao)
                .IsRequired();

            builder.Property(rt => rt.DataExpiracao)
                .IsRequired();

            builder.Property(rt => rt.Utilizado)
                .IsRequired();

            builder.Property(rt => rt.Invalidado)
                .IsRequired();

            builder.Property(rt => rt.UsuarioId)
                .IsRequired();

            // Relacionamento com Usuario
            builder.HasOne(rt => rt.Usuario)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
