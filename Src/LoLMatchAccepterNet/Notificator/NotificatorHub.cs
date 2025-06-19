using Microsoft.AspNetCore.SignalR;

namespace LoLMatchAccepterNet.Notificator
{
    public class NotificatorHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"[{Context.ConnectionId}]: Device connected to notificator!");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            Console.WriteLine($"[{Context.ConnectionId}]: Device disconnected!");
            await base.OnDisconnectedAsync(exception);
        }
    }
}
