using ArtAuction.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtAuction.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.HasIndex(c => c.Name)
            .IsUnique();

        // Relationships
        builder.HasMany(c => c.Artworks)
            .WithOne(a => a.Category)
            .HasForeignKey(a => a.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed default categories
        builder.HasData(
            new Category { Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440001"), Name = "Painting", Description = "Oil, acrylic, watercolor paintings" },
            new Category { Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440002"), Name = "Sculpture", Description = "3D sculptural works" },
            new Category { Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440003"), Name = "Photography", Description = "Digital and traditional photography" },
            new Category { Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440004"), Name = "Digital Art", Description = "NFTs and digital creations" },
            new Category { Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440005"), Name = "Jewelry", Description = "Fine jewelry and accessories" }
        );
    }
}
