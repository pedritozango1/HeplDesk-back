using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Novati.API.Models.Entities;

namespace Novati.API.Data.Configurations;

public class SolicitacaoConfiguration : IEntityTypeConfiguration<Solicitacao>
{
    public void Configure(EntityTypeBuilder<Solicitacao> b)
    {
        b.Property(s => s.Titulo).HasMaxLength(300).IsRequired();
        b.Property(s => s.Descricao).HasMaxLength(4000).IsRequired();
        b.Property(s => s.Categoria).HasMaxLength(100).IsRequired();

        // now() preenche as linhas que já existiam quando a coluna foi criada.
        b.Property(s => s.CriadoEm).HasDefaultValueSql("now()");

        // Anexos → coluna JSONB
        b.OwnsMany(s => s.Anexos, a =>
        {
            a.ToJson();
        });

        // Avaliação → owned type (colunas Avaliacao_* na própria tabela)
        b.OwnsOne(s => s.Avaliacao, av =>
        {
            av.Property(a => a.Comentario).HasMaxLength(2000);
        });

        // FK → User (solicitante, Restrict)
        b.HasOne(s => s.Solicitante)
         .WithMany()
         .HasForeignKey(s => s.SolicitanteId)
         .OnDelete(DeleteBehavior.Restrict);

        // FK → DispositivoFisico (opcional, Restrict)
        b.HasOne(s => s.DispositivoFisico)
         .WithMany()
         .HasForeignKey(s => s.DispositivoFisicoId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}