using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Novati.API.Models.Entities;

namespace Novati.API.Data.Configurations;

public class ItemStockConfiguration : IEntityTypeConfiguration<ItemStock>
{
    public void Configure(EntityTypeBuilder<ItemStock> b)
    {
        // 1 item por modelo de componente
        b.HasIndex(i => i.ModeloComponenteId).IsUnique();

        b.HasOne(i => i.ModeloComponente)
         .WithMany()
         .HasForeignKey(i => i.ModeloComponenteId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}