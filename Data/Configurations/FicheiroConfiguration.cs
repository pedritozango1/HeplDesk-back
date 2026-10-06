using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Novati.API.Models.Entities;

namespace Novati.API.Data.Configurations;

public class FicheiroConfiguration : IEntityTypeConfiguration<Ficheiro>
{
    public void Configure(EntityTypeBuilder<Ficheiro> b)
    {
        b.Property(f => f.NomeOriginal).IsRequired().HasMaxLength(255);
        b.Property(f => f.ContentType).IsRequired().HasMaxLength(100);
        b.Property(f => f.CaminhoRelativo).IsRequired().HasMaxLength(500);
        b.Property(f => f.Sha256).IsRequired().HasMaxLength(64);

        // FK → User (quem carregou). SetNull: apagar o utilizador não apaga os ficheiros.
        b.HasOne(f => f.CriadoPor)
         .WithMany()
         .HasForeignKey(f => f.CriadoPorId)
         .OnDelete(DeleteBehavior.SetNull);
    }
}
