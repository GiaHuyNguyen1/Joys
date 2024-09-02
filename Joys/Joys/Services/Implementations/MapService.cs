using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Joys.Services.Implementations
{
    public class MapService : IMapService
    {
        private WebView _mapWebView;
        private static readonly Lazy<MapService> _instance = new Lazy<MapService>(() => new MapService());

        // Private constructor để ngăn không cho tạo instance từ bên ngoài class
        private MapService()
        {
        }

        // Instance duy nhất của MapService
        public static MapService Instance => _instance.Value;

        // Hàm thiết lập WebView, cần được gọi trước khi sử dụng các hàm khác
        public void Initialize(WebView mapWebView)
        {
            _mapWebView = mapWebView;
        }

        // Hàm để load bản đồ và vị trí hiện tại
        public async Task LoadOpenStreetMapAsync()
        {
            // Kiểm tra xem WebView đã được thiết lập chưa
            if (_mapWebView == null)
            {
                WriteToScreen("MapWebView is not initialized.");
                return;
            }

            // Lấy vị trí hiện tại của thiết bị
            var location = await Geolocation.GetLastKnownLocationAsync();

            // Kiểm tra xem có lấy được vị trí không
            if (location == null)
            {
                WriteToScreen("Unable to get current location.");
                return;
            }

            // Chuyển vị trí lấy được thành chuỗi cho JavaScript
            double latitude = location.Latitude;
            double longitude = location.Longitude;

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
        let markers = [];

        function loadMap() {{
            try {{
                map = L.map('map').setView([{latitude}, {longitude}], 13); // Đặt bản đồ ở vị trí hiện tại
                L.tileLayer('https://{{s}}.tile.openstreetmap.org/{{z}}/{{x}}/{{y}}.png', {{
                    maxZoom: 19,
                    attribution: '© OpenStreetMap contributors'
                }}).addTo(map);

                // Thêm marker cho vị trí hiện tại
                addMarker({latitude}, {longitude}, 'Current Location');
            }} catch (error) {{
                console.error('Error loading map:', error);
                alert('Error loading the map. Please check your connection and try again.');
            }}
        }}

        function clearMarkers() {{
            // Xóa tất cả các marker trên bản đồ
            markers.forEach(marker => marker.remove());
            markers = [];
        }}

        function addMarker(latitude, longitude, name) {{
            // Thêm marker và hiển thị popup với tên
            const location = [latitude, longitude];
            const marker = L.marker(location).addTo(map)
                .bindPopup(`<b>${{name}}</b>`)
                .openPopup();
            markers.push(marker);
        }}
    </script>
</head>
<body onload='loadMap();'>
    <div id='map' style='width: 100%; height: 100vh;'></div>
</body>
</html>";

            // Đặt mã HTML vào WebView
            _mapWebView.Source = new HtmlWebViewSource
            {
                Html = html
            };
        }

        // Hàm để thêm marker vào bản đồ (nếu cần sau này)
        public async Task AddMarkerAsync(double latitude, double longitude, string name)
        {
            if (_mapWebView == null)
            {
                WriteToScreen("MapWebView is not initialized.");
                return;
            }

            string jsCommand = $"addMarker({latitude}, {longitude}, '{name}');";
            await _mapWebView.EvaluateJavaScriptAsync(jsCommand);
        }

        // Hàm hiển thị thông báo hoặc log ra màn hình
        private void WriteToScreen(string message)
        {
            Console.WriteLine(message); // Bạn có thể thay thế bằng code hiển thị lên giao diện nếu cần
        }
    }
}