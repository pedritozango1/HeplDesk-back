using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Novati.API.Models.Entities;

namespace Novati.API.Data.Configurations;

public class OrdemHistoricoConfiguration : IEntityTypeConfiguration<OrdemHistorico>
{
    public void Configure(EntityTypeBuilder<OrdemHistorico> b)
    {
        b.Property(h => h.Texto).HasMaxLength(2000).IsRequired();

        // FK → Ordem (Cascade)
        b.HasOne(h => h.Ordem)
         .WithMany(o => o.Historico)
         .HasForeignKey(h => h.OrdemId)
         .OnDelete(DeleteBehavior.Cascade);

        // FK → User (autor, opcional, SetNull)
        b.HasOne(h => h.Autor)
         .WithMany()
         .HasForeignKey(h => h.AutorId)
         .OnDelete(DeleteBehavior.SetNull);
    }
}
