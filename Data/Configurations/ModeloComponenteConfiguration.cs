using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Novati.API.Models.Entities;

namespace Novati.API.Data.Configurations;

public class ModeloComponenteConfiguration : IEntityTypeConfiguration<ModeloComponente>
{
    public void Configure(EntityTypeBuilder<ModeloComponente> b)
    {
        b.Property(m => m.Nome).HasMaxLength(200).IsRequired();
        b.Property(m => m.Tipo).HasMaxLength(100).IsRequired();
        b.Property(m => m.Capacidade).HasMaxLength(100).IsRequired();
        b.Property(m => m.StockMinimo).HasDefaultValue(1);
    }
}