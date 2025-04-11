using Adly.Domain.Entities.Ad;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adly.Infrastructure.Persistence.Configurations.Location;

internal class LocationEntityConfiguration:IEntityTypeConfiguration<LocationEntity>
{
    public void Configure(EntityTypeBuilder<LocationEntity> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).HasMaxLength(100);

        builder.HasIndex(c => c.Name);

        builder.HasMany(c => c.Ads)
            .WithOne(c => c.Location)
            .HasForeignKey(c => c.LocationId);

        builder.ToTable("Locations", "ad");
    }
}