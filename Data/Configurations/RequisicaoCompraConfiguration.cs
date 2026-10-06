using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Novati.API.Models.Entities;

namespace Novati.API.Data.Configurations;

public class RequisicaoCompraConfiguration : IEntityTypeConfiguration<RequisicaoCompra>
{
    public void Configure(EntityTypeBuilder<RequisicaoCompra> b)
    {
        b.Property(r => r.Justificativa).HasMaxLength(2000).IsRequired();

        // now() preenche as linhas que já existiam quando a coluna foi criada.
        b.Property(r => r.CriadoEm).HasDefaultValueSql("now()");

        // FK → ItemStock (Restrict)
        b.HasOne(r => r.ItemStock)
         .WithMany(i => i.Requisicoes)
         .HasForeignKey(r => r.ItemStockId)
         .OnDelete(DeleteBehavior.Restrict);

        // FK → ModeloComponente (Restrict)
        b.HasOne(r => r.ModeloComponente)
         .WithMany()
         .HasForeignKey(r => r.ModeloComponenteId)
         .OnDelete(DeleteBehavior.Restrict);

        // FK → User (solicitante, Restrict)
        b.HasOne(r => r.Solicitante)
         .WithMany()
         .HasForeignKey(r => r.SolicitanteId)
         .OnDelete(DeleteBehavior.Restrict);

        // FK → Ordem (opcional, SetNull)
        b.HasOne(r => r.Ordem)
         .WithMany(o => o.Requisicoes)
         .HasForeignKey(r => r.OrdemId)
         .OnDelete(DeleteBehavior.SetNull);
    }
}
