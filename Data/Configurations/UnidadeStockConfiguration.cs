using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Novati.API.Models.Entities;

namespace Novati.API.Data.Configurations;

public class UnidadeStockConfiguration : IEntityTypeConfiguration<UnidadeStock>
{
    public void Configure(EntityTypeBuilder<UnidadeStock> b)
    {
        b.Property(u => u.Codigo).HasMaxLength(50).IsRequired();

        // Código único
        b.HasIndex(u => u.Codigo).IsUnique();

        // FK → ItemStock (Restrict)
        b.HasOne(u => u.ItemStock)
         .WithMany(i => i.Unidades)
         .HasForeignKey(u => u.ItemStockId)
         .OnDelete(DeleteBehavior.Restrict);

        // FK → OrdemReparo (ReservaParaOrdem, opcional, SetNull)
        b.HasOne(u => u.ReservadaParaOrdem)
         .WithMany()
         .HasForeignKey(u => u.ReservadaParaOrdemId)
         .OnDelete(DeleteBehavior.SetNull);
    }
}