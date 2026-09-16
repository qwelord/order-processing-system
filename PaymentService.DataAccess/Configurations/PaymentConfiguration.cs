using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentService.DataAccess.Constants;
using PaymentService.DataAccess.Entities;

namespace PaymentService.DataAccess.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(payment => payment.Id);
        builder.Property(payment => payment.Amount).HasPrecision(MoneyLimits.Precision, MoneyLimits.Scale);
        builder.Property(payment => payment.Status).HasConversion<int>();
        builder.Property(payment => payment.PaymentMethod)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(PaymentLimits.PaymentMethodMaxLength);
        builder.Property(payment => payment.CardLast4)
            .HasMaxLength(PaymentLimits.CardLast4Length);
        builder.HasIndex(payment => payment.OrderId).IsUnique();
    }
}
