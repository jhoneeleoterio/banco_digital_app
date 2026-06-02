using BancoDigital.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BancoDigital.Infrastructure.Persistence.Configurations;

internal sealed class TransferenciaConfiguration : IEntityTypeConfiguration<Transferencia>
{
    public void Configure(EntityTypeBuilder<Transferencia> builder)
    {
        builder.ToTable("transferencias");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();
        builder.Property(t => t.Valor).HasPrecision(18, 2);
        builder.Property(t => t.RealizadaEm).IsRequired();
        builder.Property(t => t.Status).IsRequired().HasConversion<string>();

        builder.HasIndex(t => t.ContaOrigemId);
        builder.HasIndex(t => t.ContaDestinoId);

        builder.HasOne(t => t.ContaOrigem)
               .WithMany()
               .HasForeignKey(t => t.ContaOrigemId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.ContaDestino)
               .WithMany()
               .HasForeignKey(t => t.ContaDestinoId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
