using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Persistence.Configurations;

public class PaymentGroupConfiguration : IEntityTypeConfiguration<PaymentGroup>
{
    public void Configure(EntityTypeBuilder<PaymentGroup> builder)
    {
        builder.HasOne(g => g.Customer)
            .WithMany()
            .HasForeignKey(g => g.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(g => g.PaymentMethod)
            .WithMany()
            .HasForeignKey(g => g.PaymentMethodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(g => g.Orders)
            .WithOne(o => o.PaymentGroup)
            .HasForeignKey(o => o.PaymentGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(g => g.Status);
    }
}