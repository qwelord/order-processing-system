using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PaymentService.DataAccess;
using PaymentService.WebApi.Filters;
using PaymentService.WebApi.PipelineBehaviors;
using PaymentService.WebApi.Services;
using PaymentService.WebApi.Validators;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options => options.Filters.Add<ValidationExceptionFilter>());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

builder.Services.AddValidatorsFromAssemblyContaining<ProcessPaymentCommandValidator>();
builder.Services.AddSingleton<IKafkaProducerService, KafkaProducerService>();
builder.Services.AddHostedService<OutboxProcessorBackgroundService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
