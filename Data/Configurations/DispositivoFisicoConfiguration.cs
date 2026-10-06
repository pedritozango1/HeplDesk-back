using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Novati.API.Models.Entities;

namespace Novati.API.Data.Configurations;

public class DispositivoFisicoConfiguration: IEntityTypeConfiguration<DispositivoFisico>
{
     public void Configure(EntityTypeBuilder<DispositivoFisico> b)
    {
        b.Property(d => d.Patrimonio).HasMaxLength(50).IsRequired();
        b.Property(d => d.NumeroSerie).HasMaxLength(100).IsRequired();

        // Património único
        b.HasIndex(d => d.Patrimonio).IsUnique();

        // FK → ModeloDispositivo (Restrict)
        b.HasOne(d => d.ModeloDispositivo)
         .WithMany()
         .HasForeignKey(d => d.ModeloDispositivoId)
         .OnDelete(DeleteBehavior.Restrict);

        // FK → Localizacao (Restrict)
        b.HasOne(d => d.Localizacao)
         .WithMany()
         .HasForeignKey(d => d.LocalizacaoId)
         .OnDelete(DeleteBehavior.Restrict);

        // FK → User (responsável, opcional, SetNull)
        b.HasOne(d => d.Responsavel)
         .WithMany()
         .HasForeignKey(d => d.ResponsavelId)
         .OnDelete(DeleteBehavior.SetNull);
    }
}
