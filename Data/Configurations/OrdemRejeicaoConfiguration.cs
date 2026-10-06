using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Novati.API.Models.Entities;

namespace Novati.API.Data.Configurations;

public class OrdemRejeicaoConfiguration : IEntityTypeConfiguration<OrdemRejeicao>
{
    public void Configure(EntityTypeBuilder<OrdemRejeicao> b)
    {
        b.Property(r => r.Motivo).HasMaxLength(2000).IsRequired();

        b.HasOne(r => r.Ordem)
         .WithMany(o => o.Rejeicoes)
         .HasForeignKey(r => r.OrdemId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}