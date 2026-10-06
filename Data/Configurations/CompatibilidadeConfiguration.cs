using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Novati.API.Models.Entities;

namespace Novati.API.Data.Configurations;

public class CompatibilidadeConfiguration : IEntityTypeConfiguration<Compatibilidade>
{
    public void Configure(EntityTypeBuilder<Compatibilidade> b)
    {
        b.HasOne(u => u.ModeloComponente)
        .WithMany()
        .HasForeignKey(u => u.ModeloComponenteId)
        .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(u => u.ModeloDispositivo)
        .WithMany()
        .HasForeignKey(u => u.ModeloDispositivoId)
        .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(u => new { u.ModeloComponenteId, u.ModeloDispositivoId }).IsUnique();

    }
}
