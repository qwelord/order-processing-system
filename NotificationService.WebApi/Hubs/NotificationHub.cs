using Microsoft.AspNetCore.SignalR;

namespace NotificationService.WebApi.Hubs;

public class NotificationHub : Hub
{
    public async Task SendPaymentNotification(string message)
    {
        await Clients.All.SendAsync("ReceivePaymentUpdate", message);
    }
}