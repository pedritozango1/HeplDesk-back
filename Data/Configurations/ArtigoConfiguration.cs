using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Novati.API.Models.Entities;

namespace Novati.API.Data.Configurations;

public class ArtigoConfiguration : IEntityTypeConfiguration<Artigo>
{
    public void Configure(EntityTypeBuilder<Artigo> b)
    {
        b.Property(a => a.Titulo).HasMaxLength(300).IsRequired();
        b.Property(a => a.Conteudo).IsRequired();
        b.Property(a => a.Categoria).HasMaxLength(100).IsRequired();

        // Lista de strings → text[] nativo no Postgres
        b.Property(a => a.Tags)
         .HasColumnType("text[]");

        // FK → User (autor, Restrict)
        b.HasOne(a => a.Autor)
         .WithMany()
         .HasForeignKey(a => a.AutorId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
