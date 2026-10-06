using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Novati.API.Models.Entities;

namespace Novati.API.Data.Configurations;

public class OrdemReparoConfiguration : IEntityTypeConfiguration<OrdemReparo>
{
    public void Configure(EntityTypeBuilder<OrdemReparo> b)
    {
        b.Property(o => o.Diagnostico).HasMaxLength(4000);
        b.Property(o => o.Solucao).HasMaxLength(4000);
        b.Property(o => o.SolucaoSugerida).HasMaxLength(4000);

        // 1–1 com Solicitacao (SolicitacaoId único)
        b.HasIndex(o => o.SolicitacaoId).IsUnique();

        b.HasOne(o => o.Solicitacao)
         .WithOne(s => s.Ordem)
         .HasForeignKey<OrdemReparo>(o => o.SolicitacaoId)
         .OnDelete(DeleteBehavior.Restrict);

        // FK → User (técnico, Restrict)
        b.HasOne(o => o.Tecnico)
         .WithMany()
         .HasForeignKey(o => o.TecnicoId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}