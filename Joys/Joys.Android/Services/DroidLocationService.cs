using Android.Content;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Joys.Services;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

[assembly: Dependency(typeof(Joys.Droid.Services.DroidLocationService))]
namespace Joys.Droid.Services
{
    internal class DroidLocationService : Java.Lang.Object, ILocationService, ILocationListener
    {
        private readonly IWebSocketClient _webSocketClient;
        private LocationManager _locationManager;
        private string _locationProvider;

        public event EventHandler<Xamarin.Essentials.Location> LocationChanged;

        public DroidLocationService()
        {
            _webSocketClient = WebSocketClient.Instance;
            InitializeLocationManager();
        }

        private void InitializeLocationManager()
        {
            _locationManager = (LocationManager)Android.App.Application.Context.GetSystemService(Context.LocationService);
            _locationProvider = LocationManager.GpsProvider;
        }

        public void StartTrackingLocation()
        {
            _locationManager.RequestLocationUpdates(_locationProvider, 1000, 1, this);
        }

        public void StopTrackingLocation()
        {
            _locationManager.RemoveUpdates(this);
        }

        public async void OnLocationChanged(Android.Locations.Location location)
        {
            try
            {
                if (location == null) return;

                var essentialLocation = new Xamarin.Essentials.Location(location.Latitude, location.Longitude, location.Altitude);
                LocationChanged?.Invoke(this, essentialLocation);
                await SendLocationUpdate(essentialLocation);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnLocationChanged: {ex.Message}");
            }
        }


        public async Task SendLocationUpdate(Xamarin.Essentials.Location location)
        {
            if (_webSocketClient != null)
            {
                var message = JsonConvert.SerializeObject(new
                {
                    Latitude = location.Latitude,
                    Longitude = location.Longitude,
                    Timestamp = DateTime.UtcNow
                });

                await _webSocketClient.SendMessageAsync(message);
            }
        }

        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras) { }
        public void OnProviderEnabled(string provider) { }
        public void OnProviderDisabled(string provider) { }
    }
}
