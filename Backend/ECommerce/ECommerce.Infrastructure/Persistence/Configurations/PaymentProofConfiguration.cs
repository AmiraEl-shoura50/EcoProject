using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Persistence.Configurations;

public class PaymentProofConfiguration : IEntityTypeConfiguration<PaymentProof>
{
    public void Configure(EntityTypeBuilder<PaymentProof> builder)
    {
        builder.HasOne(p => p.Order)
            .WithMany(o => o.PaymentProofs)
            .HasForeignKey(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.PaymentGroup)
            .WithMany(g => g.Proofs)
            .HasForeignKey(p => p.PaymentGroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}