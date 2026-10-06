using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Novati.API.Models.Entities;

namespace Novati.API.Data.Configurations;

public class RelatorioTecnicoConfiguration : IEntityTypeConfiguration<RelatorioTecnico>
{
    public void Configure(EntityTypeBuilder<RelatorioTecnico> b)
    {
        b.Property(r => r.Sumario).HasMaxLength(2000);
        b.Property(r => r.Diagnostico).HasMaxLength(4000);
        b.Property(r => r.SolucaoAplicada).HasMaxLength(4000);
        b.Property(r => r.Procedimentos).HasMaxLength(4000);
        b.Property(r => r.Observacoes).HasMaxLength(4000);
        b.Property(r => r.ComentariosInternos).HasMaxLength(4000);
        b.Property(r => r.AssinaturaTecnico).HasColumnType("text");
        b.Property(r => r.AssinaturaResponsavel).HasColumnType("text");

        // Peças usadas → coluna JSONB
        b.OwnsMany(r => r.PecasUsadas, p =>
        {
            p.ToJson();
        });

        // 1–1 com Ordem (OrdemId único)
        b.HasIndex(r => r.OrdemId).IsUnique();

        b.HasOne(r => r.Ordem)
         .WithOne(o => o.Relatorio)
         .HasForeignKey<RelatorioTecnico>(r => r.OrdemId)
         .OnDelete(DeleteBehavior.Restrict);

        // FK → Ficheiro (assinatura congelada ao finalizar, Restrict)
        b.HasOne(r => r.AssinaturaTecnicoFicheiro)
         .WithMany()
         .HasForeignKey(r => r.AssinaturaTecnicoFicheiroId)
         .OnDelete(DeleteBehavior.Restrict);

        // FK → User (autor, Restrict)
        b.HasOne(r => r.Autor)
         .WithMany()
         .HasForeignKey(r => r.AutorId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
