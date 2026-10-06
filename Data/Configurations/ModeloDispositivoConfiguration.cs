using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Novati.API.Models.Entities;

namespace Novati.API.Data.Configurations;

public class ModeloDispositivoConfiguration:IEntityTypeConfiguration<ModeloDispositivo>
{
    public void Configure(EntityTypeBuilder<ModeloDispositivo> b)
    {
        b.Property(u => u.Nome).HasMaxLength(200).IsRequired();
        b.Property(u => u.Fabricante).HasMaxLength(200).IsRequired();
        b.Property(u => u.Tipo).HasMaxLength(100).IsRequired();
    }
}
