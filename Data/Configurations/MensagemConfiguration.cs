using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Novati.API.Models.Entities;

namespace Novati.API.Data.Configurations;

public class MensagemConfiguration : IEntityTypeConfiguration<Mensagem>
{
    public void Configure(EntityTypeBuilder<Mensagem> b)
    {
        b.Property(m => m.Texto).HasMaxLength(4000).IsRequired();

        // FK → Solicitacao (Cascade)
        b.HasOne(m => m.Solicitacao)
         .WithMany(s => s.Mensagens)
         .HasForeignKey(m => m.SolicitacaoId)
         .OnDelete(DeleteBehavior.Cascade);

        // FK → User (autor, Restrict)
        b.HasOne(m => m.Autor)
         .WithMany()
         .HasForeignKey(m => m.AutorId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
