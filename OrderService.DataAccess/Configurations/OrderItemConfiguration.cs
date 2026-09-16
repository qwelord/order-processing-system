using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.DataAccess.Constants;
using OrderService.DataAccess.Entities;

namespace OrderService.DataAccess.Configurations;

public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(item => item.Id);
        builder.Property(item => item.ProductName)
            .IsRequired()
            .HasMaxLength(OrderLimits.ProductNameMaxLength);
        builder.Property(item => item.UnitPrice).HasPrecision(MoneyLimits.Precision, MoneyLimits.Scale);
        builder.Property(item => item.LineTotal).HasPrecision(MoneyLimits.Precision, MoneyLimits.Scale);
        builder.Property(item => item.Quantity).IsRequired();
        builder.HasIndex(item => item.OrderId);
        builder.HasOne(item => item.Order)
            .WithMany(order => order.Items)
            .HasForeignKey(item => item.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
