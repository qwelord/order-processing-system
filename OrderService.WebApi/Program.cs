using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.DataAccess;
using OrderService.WebApi.Clients;
using OrderService.WebApi.Filters;
using OrderService.WebApi.PipelineBehaviors;
using OrderService.WebApi.Validators;
using Refit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

builder.Services.AddControllers(options => options.Filters.Add<ValidationExceptionFilter>());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});

builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderDtoValidator>();
builder.Services.AddAutoMapper(typeof(Program).Assembly);

var paymentServiceUrl = builder.Configuration["PaymentService:BaseUrl"] ?? "http://localhost:5002";
builder.Services.AddHttpClient("PaymentClient", c => c.BaseAddress = new Uri(paymentServiceUrl));
builder.Services.AddTransient(sp =>
{
    var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient("PaymentClient");
    return RestService.For<IPaymentClient>(client);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    db.Database.Migrate();
    if (!db.Products.Any())
    {
        db.Products.AddRange(
            new OrderService.DataAccess.Entities.Product { Id = Guid.NewGuid(), Name = "Aurora Laptop 14", Description = "Lightweight 14-inch laptop for work and study.", Price = 1199.00m, StockQuantity = 12, IsActive = true, CreatedAt = DateTime.UtcNow },
            new OrderService.DataAccess.Entities.Product { Id = Guid.NewGuid(), Name = "Orbit Mechanical Keyboard", Description = "Compact mechanical keyboard with hot-swappable switches.", Price = 129.00m, StockQuantity = 28, IsActive = true, CreatedAt = DateTime.UtcNow },
            new OrderService.DataAccess.Entities.Product { Id = Guid.NewGuid(), Name = "Pulse Wireless Mouse", Description = "Ergonomic wireless mouse with silent clicks.", Price = 59.90m, StockQuantity = 36, IsActive = true, CreatedAt = DateTime.UtcNow },
            new OrderService.DataAccess.Entities.Product { Id = Guid.NewGuid(), Name = "Nova USB-C Dock", Description = "Multi-port dock with HDMI, USB and Ethernet.", Price = 89.00m, StockQuantity = 19, IsActive = true, CreatedAt = DateTime.UtcNow },
            new OrderService.DataAccess.Entities.Product { Id = Guid.NewGuid(), Name = "Echo Headphones", Description = "Closed-back wireless headphones with ANC.", Price = 179.00m, StockQuantity = 16, IsActive = true, CreatedAt = DateTime.UtcNow },
            new OrderService.DataAccess.Entities.Product { Id = Guid.NewGuid(), Name = "Flux Monitor 27", Description = "27-inch QHD monitor with a 100 Hz panel.", Price = 299.00m, StockQuantity = 9, IsActive = true, CreatedAt = DateTime.UtcNow });
        db.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
