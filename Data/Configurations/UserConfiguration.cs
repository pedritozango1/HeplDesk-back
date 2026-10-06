using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Novati.API.Models.Entities;

namespace Novati.API.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.Property(u => u.Nome).IsRequired().HasMaxLength(200);
        b.Property(u => u.Email).IsRequired().HasMaxLength(200);
        b.Property(u => u.PasswordHash).IsRequired().HasMaxLength(500);
        // FK → Ficheiro (assinatura). Restrict: um ficheiro em uso não se apaga.
        b.HasOne(u => u.AssinaturaFicheiro)
         .WithMany()
         .HasForeignKey(u => u.AssinaturaFicheiroId)
         .OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(u => u.Email).IsUnique();
    }   
}
