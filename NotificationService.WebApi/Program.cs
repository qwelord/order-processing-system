using NotificationService.WebApi.Hubs;
using NotificationService.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddSignalR();
builder.Services.AddHostedService<KafkaConsumerService>();

var app = builder.Build();

app.UseCors("AllowAll");

app.MapHub<NotificationHub>("/notifications");

app.Run();