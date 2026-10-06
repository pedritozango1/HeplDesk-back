using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Novati.API.Models.Entities;

namespace Novati.API.Data.Configurations;

public class ArtigoAvaliacaoConfiguration : IEntityTypeConfiguration<ArtigoAvaliacao>
{
    public void Configure(EntityTypeBuilder<ArtigoAvaliacao> b)
    {
        // Uma avaliação por utilizador e artigo.
        b.HasIndex(a => new { a.ArtigoId, a.UserId }).IsUnique();

        // FK → Artigo (Cascade: apagar o artigo apaga as suas avaliações)
        b.HasOne(a => a.Artigo)
         .WithMany(x => x.Avaliacoes)
         .HasForeignKey(a => a.ArtigoId)
         .OnDelete(DeleteBehavior.Cascade);

        // FK → User (Cascade: as avaliações não têm valor sem o utilizador)
        b.HasOne(a => a.User)
         .WithMany()
         .HasForeignKey(a => a.UserId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}
