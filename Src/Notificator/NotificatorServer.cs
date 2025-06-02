using LoLMatchAccepter.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoLMatchAccepterNet.Api
{
    public sealed class NotificatorServer
    {
        private readonly IWebHost _host;
        private readonly IHubContext<NotificatorHub> _notificationHub;
        public NotificatorServer()
        {
            var builder = new WebHostBuilder();

            builder.UseKestrel(opt =>
            {
                opt.ListenAnyIP(NotificationServerContants.HubPort);
            })
            .ConfigureServices(services =>
            {
                services.AddSignalR();
            })
            .Configure(app =>
            {
                app.UseSignalR(signalR =>
                {
                    signalR.MapHub<NotificatorHub>($"/{NotificationServerContants.HubRoute}");
                });
            });

            _host = builder.Build();
            _notificationHub = _host.Services.GetRequiredService<IHubContext<NotificatorHub>>();
            _host.Start();
            Console.WriteLine($"Started SignalR notificator hub on port {NotificationServerContants.HubPort}.");
        }

        public void Stop()
        {
            _host.StopAsync().GetAwaiter().GetResult();
            _host.Dispose();
        }

        public Task SendNotification()
        {
            return _notificationHub.Clients.All.SendAsync(NotificationServerContants.NotificationEventName);
        }
    }
}
