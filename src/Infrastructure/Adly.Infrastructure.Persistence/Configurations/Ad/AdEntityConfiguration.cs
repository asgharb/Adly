using Adly.Domain.Entities.Ad;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Adly.Infrastructure.Persistence.Configurations.Ad;

internal class AdEntityConfiguration:IEntityTypeConfiguration<AdEntity>
{
    public void Configure(EntityTypeBuilder<AdEntity> builder)
    {
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.Title).HasMaxLength(100);

        builder.Property(c => c.Description).HasMaxLength(2048);

        builder.HasOne(c => c.Category)
            .WithMany(c => c.Ads)
            .HasForeignKey(c => c.CategoryId);

        builder.HasOne(c => c.Location)
            .WithMany(c => c.Ads)
            .HasForeignKey(c => c.LocationId);

        builder.HasOne(c => c.User)
            .WithMany(c => c.Ads)
            .HasForeignKey(c => c.UserId);

        builder.Property(c => c.CurrentState).HasConversion<EnumToStringConverter<AdEntity.AdStates>>();

        builder.Property(c => c.CurrentState).HasMaxLength(20);

        builder.HasIndex(c => c.Title);

        builder.HasIndex(c => c.CurrentState);

        builder.OwnsMany(c => c.Images, navigationBuilder =>
        {
            navigationBuilder.ToJson("Images");
        });

        builder.OwnsMany(c => c.ChangeLogs, navigationBuilder =>
        {
            navigationBuilder.ToJson("ChangeLogs");
        });

        builder.ToTable("Ads", "ad");

    }
}