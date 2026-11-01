using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasOne(c => c.ParentCategory)
            .WithMany(c => c.SubCategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // ✅ جديد: علاقة اختيارية مع البائع المنشئ
        builder.HasOne<Seller>()
            .WithMany()
            .HasForeignKey(c => c.CreatedBySellerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
