using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Novati.API.Models.Entities;

namespace Novati.API.Data.Configurations;

public class NotificacaoConfiguration : IEntityTypeConfiguration<Notificacao>
{
    public void Configure(EntityTypeBuilder<Notificacao> b)
    {
        b.Property(n => n.Message).HasMaxLength(500).IsRequired();
        b.Property(n => n.Link).HasMaxLength(500);

        // FK → User (Cascade)
        b.HasOne(n => n.User)
         .WithMany()
         .HasForeignKey(n => n.UserId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}
