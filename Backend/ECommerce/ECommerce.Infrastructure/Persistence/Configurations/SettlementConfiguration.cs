using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Persistence.Configurations;

public class SettlementConfiguration : IEntityTypeConfiguration<Settlement>
{
    public void Configure(EntityTypeBuilder<Settlement> builder)
    {
        builder.Property(s => s.GrossAmount).HasColumnType("decimal(18,2)");
        builder.Property(s => s.CommissionAmount).HasColumnType("decimal(18,2)");
        builder.Property(s => s.AdjustmentAmount).HasColumnType("decimal(18,2)");
        builder.Property(s => s.NetAmount).HasColumnType("decimal(18,2)");
        builder.Property(s => s.CommissionPercent).HasColumnType("decimal(5,2)");

        builder.Property(s => s.PayoutMethod).IsRequired().HasMaxLength(30);
        builder.Property(s => s.PayoutAccount).IsRequired().HasMaxLength(100);
        builder.Property(s => s.Reference).HasMaxLength(100);
        builder.Property(s => s.AdjustmentNote).HasMaxLength(300);

        builder.HasOne(s => s.Seller)
            .WithMany()
            .HasForeignKey(s => s.SellerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.Orders)
            .WithOne(o => o.Settlement)
            .HasForeignKey(o => o.SettlementId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => new { s.SellerId, s.CreatedAt });
    }
}