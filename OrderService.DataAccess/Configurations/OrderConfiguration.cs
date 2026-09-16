using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.DataAccess.Constants;
using OrderService.DataAccess.Entities;

namespace OrderService.DataAccess.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(order => order.Id);
        builder.Property(order => order.CustomerName)
            .IsRequired()
            .HasMaxLength(OrderLimits.CustomerNameMaxLength);
        builder.Property(order => order.CustomerEmail)
            .IsRequired()
            .HasMaxLength(OrderLimits.CustomerEmailMaxLength);
        builder.Property(order => order.PaymentMethod)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(OrderLimits.PaymentMethodMaxLength);
        builder.Property(order => order.TotalAmount).HasPrecision(MoneyLimits.Precision, MoneyLimits.Scale);
        builder.Property(order => order.Status).HasConversion<int>();
        builder.Property(order => order.CreatedAt).IsRequired();
    }
}
