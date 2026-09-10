using Microsoft.AspNetCore.SignalR;

namespace NotificationService.WebApi.Hubs;

public class NotificationHub : Hub<INotificationClient>
{
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }
}