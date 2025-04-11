using Adly.Domain.Entities.Ad;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adly.Infrastructure.Persistence.Configurations.Category;

internal class CategoryEntityConfiguration:IEntityTypeConfiguration<CategoryEntity>
{
    public void Configure(EntityTypeBuilder<CategoryEntity> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).HasMaxLength(100);

        builder.HasIndex(c => c.Name);

        builder.HasMany(c => c.Ads)
            .WithOne(c => c.Category)
            .HasForeignKey(c => c.CategoryId);

        builder.ToTable("Categories", "ad");
    }
}