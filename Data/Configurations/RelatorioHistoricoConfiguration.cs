using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Novati.API.Models.Entities;

namespace Novati.API.Data.Configurations;

public class RelatorioHistoricoConfiguration : IEntityTypeConfiguration<RelatorioHistorico>
{
    public void Configure(EntityTypeBuilder<RelatorioHistorico> b)
    {
        b.Property(h => h.Campo).HasMaxLength(100);
        b.Property(h => h.ValorAntigo).HasMaxLength(4000);
        b.Property(h => h.ValorNovo).HasMaxLength(4000);

        // FK → Relatorio (Cascade)
        b.HasOne(h => h.Relatorio)
         .WithMany(r => r.Historico)
         .HasForeignKey(h => h.RelatorioId)
         .OnDelete(DeleteBehavior.Cascade);

        // FK → User (autor, Restrict)
        b.HasOne(h => h.Autor)
         .WithMany()
         .HasForeignKey(h => h.AutorId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
