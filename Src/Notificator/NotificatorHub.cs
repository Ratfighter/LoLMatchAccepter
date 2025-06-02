using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoLMatchAccepterNet.Api
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
