using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Novati.API.Models.Entities;

namespace Novati.API.Data.Configurations;

public class InstanciaComponenteConfiguration : IEntityTypeConfiguration<InstanciaComponente>
{
    public void Configure(EntityTypeBuilder<InstanciaComponente> b)
    {
        b.Property(i => i.Codigo).HasMaxLength(100).IsRequired();

        // FK → DispositivoFisico (Cascade)
        b.HasOne(i => i.DispositivoFisico)
         .WithMany(d => d.Instancias)
         .HasForeignKey(i => i.DispositivoFisicoId)
         .OnDelete(DeleteBehavior.Cascade);

        // FK → ModeloComponente (Restrict)
        b.HasOne(i => i.ModeloComponente)
         .WithMany()
         .HasForeignKey(i => i.ModeloComponenteId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}