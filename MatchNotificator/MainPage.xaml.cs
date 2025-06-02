using LoLMatchAccepter.Common;
using MatchNotificator.Services;
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

        private void OnConnectClicked(object sender, EventArgs e)
        {
            string ipAddressText = IpInput.Text.Trim();
            if (!IPAddress.TryParse(ipAddressText, out _))
            {
                ClearEntry();
                return;
            }
            ConnectToMatchAccepter(ipAddressText);
        }

        private void OnDisconnectClicked(object sender, EventArgs e)
        {
            StopHub();
            ClearEntry();
            DisconnectedState();
        }

        private async void ConnectToMatchAccepter(string ipAddressText, bool showPopup = true)
        {
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
                await connection.StartAsync();
                ConnectedState();
                Preferences.Set(IpAddressPreferenceKey, ipAddressText);
                connection.On(NotificationServerContants.NotificationEventName, () =>
                {
                    NotificationManagerService.Instance.NotifyUser();
                });

            }
            catch (Exception exc)
            {
                ClearEntry();
                if (showPopup)
                {
                    await DisplayAlert("Error", $"Could not connect to {signalrHub}.\nCause: {exc.Message}", "Ok");
                }
            }
        }

        private void ClearEntry()
        {
            IpInput.Text = string.Empty;
            Preferences.Remove(IpAddressPreferenceKey);
        }

        private void ConnectedState()
        {
            IpInput.IsVisible = false;
            ConnectBtn.IsVisible = false;
            DisconnectBtn.IsVisible = true;
            StatusLabel.Text = "Connected to Lol Match Accepter!";
        }

        private void DisconnectedState()
        {
            IpInput.IsVisible = true;
            ConnectBtn.IsVisible = true;
            DisconnectBtn.IsVisible = false;
            StatusLabel.Text = "Connect to LolMatchAccepter to get notifications!";
        }

        private void StopHub()
        {
            if(connection == null)
            {
                return;
            }
            if(connection.State != HubConnectionState.Disconnected)
            {
                connection.StopAsync().GetAwaiter().GetResult();
            }
            ValueTask? connectionDisposal = connection.DisposeAsync();
            if (connectionDisposal.HasValue)
            {
                connectionDisposal.Value.GetAwaiter().GetResult();
            }
            connection = null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    StopHub();
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
