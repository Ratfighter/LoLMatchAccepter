using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoLMatchAccepterNet.Api
{
    public class Notificator
    {
        public const string NotificatorUrl = "http://localhost:15000";
        private readonly IWebHost _host;
        protected Notificator()
        {
            var builder = new WebHostBuilder();

            builder.UseUrls(NotificatorUrl)
                .ConfigureServices(services => {
                    services.AddSignalRCore();
                })
                .Configure(app =>
                {
                    app.UseSignalR(signalR =>
                    {
                        signalR.MapHub<NotificatorHub>("/");
                    });
                });

            _host = builder.Build();
        }

        public void Start()
        {
            _host.Start();
        }

        public void Stop()
        {
            _host.StopAsync().GetAwaiter().GetResult();
            _host.Dispose();
        }
    }
}
