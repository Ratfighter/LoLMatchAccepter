using LoLMatchAccepter.Common;
using MatchNotificator.Services;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net;

namespace MatchNotificator
{
    public partial class MainPage : ContentPage, IDisposable
    {
        private const string IpAddressPreferenceKey = "ipaddress";

        private HubConnection? connection;
        private bool disposedValue;

        public MainPage()
        {
            InitializeComponent();
            string ipAddress = Preferences.Get(IpAddressPreferenceKey, string.Empty);
            if (string.IsNullOrEmpty(ipAddress))
            {
                DisconnectedState();
            }
            else
            {
                IpInput.Text = ipAddress;
            }

        }

        private async void OnConnectClicked(object sender, EventArgs e)
        {
            string ipAddressText = IpInput.Text.Trim();
            if (!IPAddress.TryParse(ipAddressText, out _))
            {
                ClearEntry();
                return;
            }
            if (string.IsNullOrWhiteSpace(ipAddressText))
            {
                return;
            }
            string signalrHub = $"http://{ipAddressText}:{NotificationServerContants.HubPort}/{NotificationServerContants.HubRoute}";
            connection = new HubConnectionBuilder()
                .WithUrl(signalrHub)
                .Build();

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                ConnectBtn.IsEnabled = false;
                await connection.StartAsync(cts.Token);
                ConnectedState();
                Preferences.Set(IpAddressPreferenceKey, ipAddressText);
                connection.On(NotificationServerContants.NotificationEventName, () =>
                {
                    NotificationManagerService.Instance.NotifyUser();
                });
                connection.Closed += (error) =>
                {
                    Dispatcher.Dispatch(async () =>
                    {
                        await DisplayAlert("Connection Closed", $"Connection to {signalrHub} was closed.", "Ok");
                        DisconnectedState();
                    });
                    return Task.CompletedTask;
                };

            }
            catch (Exception exc)
            {
                await DisplayAlert("Error", $"Could not connect to {signalrHub}.", "Ok");
                ConnectBtn.IsEnabled = true;
            }
        }

        private void OnDisconnectClicked(object sender, EventArgs e)
        {
            StopHub();
        }

        private void ClearEntry()
        {
            IpInput.Text = string.Empty;
            Preferences.Remove(IpAddressPreferenceKey);
        }

        private void ConnectedState()
        {
            IpInput.IsVisible = false;
            ConnectBtn.IsEnabled = false;
            ConnectBtn.IsVisible = false;
            DisconnectBtn.IsVisible = true;
            DisconnectBtn.IsEnabled = true;
            StatusLabel.Text = "Connected to Lol Match Accepter!";
        }

        private void DisconnectedState()
        {
            IpInput.IsVisible = true;
            ConnectBtn.IsEnabled = true;
            ConnectBtn.IsVisible = true;
            DisconnectBtn.IsVisible = false;
            StatusLabel.Text = "Connect to LolMatchAccepter to get notifications!";
        }

        private void StopHub()
        {
            ArgumentNullException.ThrowIfNull(connection);
            if(connection.State != HubConnectionState.Disconnected)
            {
                connection.StopAsync().GetAwaiter().GetResult();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if(connection != null)
                    {
                        StopHub();
                        ValueTask? connectionDisposal = connection.DisposeAsync();
                        if (connectionDisposal.HasValue)
                        {
                            connectionDisposal.Value.GetAwaiter().GetResult();
                        }
                    }
                }

                connection = null;
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
