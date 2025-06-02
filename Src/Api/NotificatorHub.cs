using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoLMatchAccepterNet.Api
{
    class NotificatorHub : Hub
    {

        public override Task OnConnectedAsync()
        {
            Console.WriteLine("Device connected to Notificator!");
            return Task.CompletedTask;
        }

        public void Notify(string message)
        {
            Console.WriteLine($"Notification received: {message}");
            Clients.All.SendAsync("ReceiveNotification", message);
        }
    }
}
