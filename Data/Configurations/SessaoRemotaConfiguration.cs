using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Novati.API.Models.Entities;

namespace Novati.API.Data.Configurations;

public class SessaoRemotaConfiguration : IEntityTypeConfiguration<SessaoRemota>
{
    public void Configure(EntityTypeBuilder<SessaoRemota> b)
    {
        b.Property(s => s.TokenHash).HasMaxLength(64);
        b.Property(s => s.MotivoFim).HasMaxLength(200);
        b.Property(s => s.AutorizadaIp).HasMaxLength(45);
        b.Property(s => s.AgentePc).HasMaxLength(64);

        // Propriedade calculada, não é coluna.
        b.Ignore(s => s.EmCurso);

        // uint + IsRowVersion = coluna de sistema xmin do PostgreSQL (não cria coluna nova).
        // O EF acrescenta "WHERE xmin = <lido>" a cada UPDATE: se outro pedido gravou primeiro,
        // 0 linhas afetadas → DbUpdateConcurrencyException.
        b.Property(s => s.Versao).IsRowVersion();

        // Uma só sessão em curso por solicitação — garantido pela BD, mesmo com pedidos simultâneos.
        b.HasIndex(s => s.SolicitacaoId)
         .IsUnique()
         .HasFilter("\"Estado\" IN ('PEDIDA', 'AUTORIZADA', 'ATIVA')");

        // FK → Solicitacao (Cascade)
        b.HasOne(s => s.Solicitacao)
         .WithMany()
         .HasForeignKey(s => s.SolicitacaoId)
         .OnDelete(DeleteBehavior.Cascade);

        // FK → User (técnico, Restrict)
        b.HasOne(s => s.Tecnico)
         .WithMany()
         .HasForeignKey(s => s.TecnicoId)
         .OnDelete(DeleteBehavior.Restrict);

        // FK → User (solicitante, Restrict)
        b.HasOne(s => s.Solicitante)
         .WithMany()
         .HasForeignKey(s => s.SolicitanteId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
