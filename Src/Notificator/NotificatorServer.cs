using LoLMatchAccepter.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoLMatchAccepterNet.Api
{
    public sealed class NotificatorServer : IDisposable
    {
        private readonly IWebHost _host;
        private readonly IHubContext<NotificatorHub> _notificationHub;
        private bool disposedValue;

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

        private void Stop()
        {
            _host.StopAsync().GetAwaiter().GetResult();
            _host.Dispose();
        }

        public async void SendNotification(object? sender, EventArgs e)
        {
            await _notificationHub.Clients.All.SendAsync(NotificationServerContants.NotificationEventName);
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Stop();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
