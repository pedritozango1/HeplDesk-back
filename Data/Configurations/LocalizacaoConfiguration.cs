using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Novati.API.Models.Entities;

namespace Novati.API.Data.Configurations;

public class LocalizacaoConfiguration : IEntityTypeConfiguration<Localizacao>
{
    public void Configure(EntityTypeBuilder<Localizacao> b)
    {
        b.Property(l => l.Nome).HasMaxLength(200).IsRequired();
        b.HasOne(u => u.Pai)
        .WithMany(l => l.Filhos)
        .HasForeignKey(u => u.PaiId)
        .OnDelete(DeleteBehavior.Restrict);
    }
}
