using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.DataAccess.Constants;
using OrderService.DataAccess.Entities;

namespace OrderService.DataAccess.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(product => product.Id);
        builder.Property(product => product.Name)
            .IsRequired()
            .HasMaxLength(OrderLimits.ProductNameMaxLength);
        builder.Property(product => product.Description)
            .HasMaxLength(OrderLimits.ProductDescriptionMaxLength);
        builder.Property(product => product.Price).HasPrecision(MoneyLimits.Precision, MoneyLimits.Scale);
        builder.Property(product => product.StockQuantity).IsRequired();
        builder.Property(product => product.IsActive).IsRequired();
        builder.Property(product => product.CreatedAt).IsRequired();
        builder.HasIndex(product => product.Name);
        builder.HasIndex(product => product.IsActive);
    }
}
