using APIProject.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace APIProject.Infrastructure.Persistencia.Configuracoes
{
    public class UsuarioConfiguracao : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Nome)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(u => u.Email)
                .IsUnique();

            builder.Property(u => u.Senha)
                .IsRequired();

            builder.Property(u => u.DataCriacao)
                .IsRequired();

            builder.Property(u => u.UltimoLogin);

            builder.Property(u => u.Ativo)
                .IsRequired();

            // Configurar o campo RowVersion para controle de concorrência
            builder.Property(u => u.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            // Remover configuração de relacionamento com Perfil
        }
    }
}