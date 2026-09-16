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

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:5003", "http://127.0.0.1:5003"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader();
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
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderCommandValidator>();

var paymentServiceUrl = builder.Configuration["PaymentService:BaseUrl"] ?? "http://localhost:5002";
builder.Services
    .AddRefitClient<IPaymentClient>()
    .ConfigureHttpClient(client => client.BaseAddress = new Uri(paymentServiceUrl));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    db.Database.Migrate();

    if (!db.Products.Any())
    {
        db.Products.AddRange(
            OrderService.DataAccess.Entities.Product.Create(
                "Aurora Laptop 14",
                "Lightweight 14-inch laptop for work and study.",
                1199.00m,
                12,
                DateTime.UtcNow),
            OrderService.DataAccess.Entities.Product.Create(
                "Orbit Mechanical Keyboard",
                "Compact mechanical keyboard with hot-swappable switches.",
                129.00m,
                28,
                DateTime.UtcNow),
            OrderService.DataAccess.Entities.Product.Create(
                "Pulse Wireless Mouse",
                "Ergonomic wireless mouse with silent clicks.",
                59.90m,
                36,
                DateTime.UtcNow),
            OrderService.DataAccess.Entities.Product.Create(
                "Nova USB-C Dock",
                "Multi-port dock with HDMI, USB and Ethernet.",
                89.00m,
                19,
                DateTime.UtcNow),
            OrderService.DataAccess.Entities.Product.Create(
                "Echo Headphones",
                "Closed-back wireless headphones with ANC.",
                179.00m,
                16,
                DateTime.UtcNow),
            OrderService.DataAccess.Entities.Product.Create(
                "Flux Monitor 27",
                "27-inch QHD monitor with a 100 Hz panel.",
                299.00m,
                9,
                DateTime.UtcNow));

        db.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Frontend");
app.UseAuthorization();
app.MapControllers();

app.Run();
