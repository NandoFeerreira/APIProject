﻿using APIProject.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APIProject.Infrastructure.Persistencia.Configuracoes
{
    public class TokenInvalidadoConfiguracao : IEntityTypeConfiguration<TokenInvalidado>
    {
        public void Configure(EntityTypeBuilder<TokenInvalidado> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Jti)
                .IsRequired();

            builder.Property(t => t.Token)
                .IsRequired();

            builder.Property(t => t.UsuarioId)
                .IsRequired();

            builder.Property(t => t.DataExpiracao)
                .IsRequired();

            builder.Property(t => t.DataInvalidacao)
                .IsRequired();

            // Relacionamento com Usuario
            builder.HasOne(t => t.Usuario)
                .WithMany()
                .HasForeignKey(t => t.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
