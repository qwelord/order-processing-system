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

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationExceptionFilter>();
});

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

var paymentServiceUrl = builder.Configuration["PaymentService:BaseUrl"];
builder.Services.AddHttpClient("PaymentClient", c => c.BaseAddress = new Uri(paymentServiceUrl!));
builder.Services.AddTransient(sp =>
{
    var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient("PaymentClient");
    return RestService.For<IPaymentClient>(client);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();