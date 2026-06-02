using BancoDigital.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BancoDigital.Infrastructure.Persistence.Configurations;

internal sealed class ContaConfiguration : IEntityTypeConfiguration<Conta>
{
    public void Configure(EntityTypeBuilder<Conta> builder)
    {
        builder.ToTable("contas");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();
        builder.Property(c => c.Numero).IsRequired().HasMaxLength(20);
        builder.Property(c => c.Saldo).HasPrecision(18, 2);
        builder.HasIndex(c => c.Numero).IsUnique();

        builder.HasOne(c => c.Cliente)
               .WithMany()
               .HasForeignKey(c => c.ClienteId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
