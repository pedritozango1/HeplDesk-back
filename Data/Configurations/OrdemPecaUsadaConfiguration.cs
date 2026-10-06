using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Novati.API.Models.Entities;

namespace Novati.API.Data.Configurations;

public class OrdemPecaUsadaConfiguration : IEntityTypeConfiguration<OrdemPecaUsada>
{
    public void Configure(EntityTypeBuilder<OrdemPecaUsada> b)
    {
        // FK → Ordem (Cascade)
        b.HasOne(p => p.Ordem)
         .WithMany(o => o.PecasUsadas)
         .HasForeignKey(p => p.OrdemId)
         .OnDelete(DeleteBehavior.Cascade);

        // FK → UnidadeStock (Restrict)
        b.HasOne(p => p.UnidadeStock)
         .WithMany()
         .HasForeignKey(p => p.UnidadeStockId)
         .OnDelete(DeleteBehavior.Restrict);

        // FK → ModeloComponente (Restrict)
        b.HasOne(p => p.ModeloComponente)
         .WithMany()
         .HasForeignKey(p => p.ModeloComponenteId)
         .OnDelete(DeleteBehavior.Restrict);

        // FK → InstanciaComponente (opcional, SetNull)
        b.HasOne(p => p.InstaladoInstancia)
         .WithMany()
         .HasForeignKey(p => p.InstaladoInstanciaId)
         .OnDelete(DeleteBehavior.SetNull);
    }
}