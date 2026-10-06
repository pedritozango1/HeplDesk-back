using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Novati.API.Models.Entities;

namespace Novati.API.Data.Configurations;

public class MovimentoStockConfiguration : IEntityTypeConfiguration<MovimentoStock>
{
    public void Configure(EntityTypeBuilder<MovimentoStock> b)
    {
        b.Property(m => m.Observacao).HasMaxLength(500);

        // now() preenche as linhas que já existiam quando a coluna foi criada.
        b.Property(m => m.CriadoEm).HasDefaultValueSql("now()");

        // FK → ItemStock (Restrict)
        b.HasOne(m => m.ItemStock)
         .WithMany(i => i.Movimentos)
         .HasForeignKey(m => m.ItemStockId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
