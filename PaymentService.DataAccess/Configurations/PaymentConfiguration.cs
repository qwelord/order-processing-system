using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentService.DataAccess.Entities;

namespace PaymentService.DataAccess.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(payment => payment.Id);
        builder.Property(payment => payment.Amount).HasPrecision(18, 2);
        builder.Property(payment => payment.Status).HasConversion<int>();
        builder.Property(payment => payment.PaymentMethod).IsRequired().HasMaxLength(50);
        builder.Property(payment => payment.CardLast4).HasMaxLength(4);
        builder.HasIndex(payment => payment.OrderId).IsUnique();
    }
}
