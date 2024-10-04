using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Essentials;
using Newtonsoft.Json;
using Joys.Models;
using Joys.Services;

namespace Joys
{
    public partial class MainPage : ContentPage
    {
        private readonly IWebSocketClient _webSocketClient;
        private readonly ILocationService _locationService;

        private const string WebSocketUrl = "ws://192.168.68.157:49152/Echo";

        public MainPage()
        {
            InitializeComponent();

            _webSocketClient = WebSocketClient.Instance; // Sử dụng instance duy nhất của WebSocketClient
            _locationService = DependencyService.Get<ILocationService>(); // Sử dụng LocationService qua DependencyService

            Init();
        }

        private void Init()
        {
            // Khởi động dịch vụ theo dõi vị trí
            _locationService.LocationChanged += OnLocationChanged;
            _locationService.StartTrackingLocation();

            // Khởi động WebSocket khi mở ứng dụng
            OnConnectClicked(this, EventArgs.Empty);
        }

        // Xử lý khi vị trí thay đổi
        private void OnLocationChanged(object sender, Xamarin.Essentials.Location location)
        {
            // Cập nhật WebView với vị trí mới
            UpdateMap(location.Latitude, location.Longitude);
        }

        // Hàm cập nhật bản đồ với vị trí mới
        private void UpdateMap(double latitude, double longitude)
        {
            string html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8' />
    <meta name='viewport' content='initial-scale=1.0, user-scalable=no' />
    <style>
        html, body, #map {{
            width: 100%;
            height: 100%;
            margin: 0;
            padding: 0;
        }}
    </style>
    <link rel='stylesheet' href='https://unpkg.com/leaflet/dist/leaflet.css' />
    <script src='https://unpkg.com/leaflet/dist/leaflet.js'></script>
    <script>
        let map;
        let markers = {{}};

        document.addEventListener('DOMContentLoaded', function() {{
            loadMap();
        }});

        function loadMap() {{
            try {{
                map = L.map('map').setView([{latitude}, {longitude}], 13);
                L.tileLayer('https://{{s}}.tile.openstreetmap.org/{{z}}/{{x}}/{{y}}.png', {{
                    maxZoom: 19,
                    attribution: '© OpenStreetMap contributors'
                }}).addTo(map);

                addMarkers([{{
                    latitude: {latitude}, 
                    longitude: {longitude}, 
                    name: 'Current Location'
                }}]);
            }} catch (error) {{
                console.error('Error loading map:', error);
                alert('Error loading the map. Please check your connection and try again.');
            }}
        }}

        function clearMarkers() {{
            for (const key in markers) {{
                markers[key].remove();
            }}
            markers = {{}};
        }}

        function addMarkers(slaves) {{
            clearMarkers();
            slaves.forEach(slave => {{
                if (slave.latitude && slave.longitude) {{
                    addOrUpdateMarker(slave.latitude, slave.longitude, slave.name);
                }}
            }});
        }}

        function addOrUpdateMarker(latitude, longitude, name) {{
            if (markers[name]) {{
                markers[name].setLatLng([latitude, longitude]);
                console.log(`Updated marker for: ${{name}} at (${{latitude}}, ${{longitude}})`);
            }} else {{
                const marker = L.marker([latitude, longitude]).addTo(map)
                    .bindPopup(`<b>${{name}}</b>`)
                    .openPopup();
                markers[name] = marker;
                console.log(`Added marker for: ${{name}} at (${{latitude}}, ${{longitude}})`);
            }}
        }}
    </script>
</head>
<body>
    <div id='map' style='width: 100%; height: 100vh;'></div>
</body>
</html>";

            MapWebView.Source = new HtmlWebViewSource
            {
                Html = html
            };
        }

        private async void OnConnectClicked(object sender, EventArgs e)
        {
            await _webSocketClient.ConnectAsync(WebSocketUrl);
        }

        private async void OnDisconnectClicked(object sender, EventArgs e)
        {
            await _webSocketClient.DisconnectAsync();
        }

        private async void OnSendLocationClicked(object sender, EventArgs e)
        {
            await _webSocketClient.SendLocationAsync();
        }

        // Dừng theo dõi vị trí khi trang bị đóng
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _locationService.StopTrackingLocation();
        }
    }
}
