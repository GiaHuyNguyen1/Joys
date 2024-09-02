using Joys.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Xamarin.Essentials;

namespace Joys.Services
{
    public class LocationService : ILocationService
    {
        private readonly IWebSocketClient _webSocketClient;
        private const double MovementThreshold = 0.0001; // Ngưỡng phát hiện thay đổi vị trí
        private Timer _timer; // Timer để theo dõi vị trí định kỳ
        private Location _lastLocation;

        public event EventHandler<Location> LocationChanged;

        public LocationService(IWebSocketClient webSocketClient)
        {
            _webSocketClient = webSocketClient;
        }

        // Bắt đầu theo dõi vị trí của thiết bị Slave
        public void StartTrackingLocation()
        {
            // Khởi động timer để cập nhật vị trí mỗi 5 giây
            _timer = new Timer(async _ =>
            {
                await UpdateLocation();
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        }

        // Dừng theo dõi vị trí
        public void StopTrackingLocation()
        {
            _timer?.Change(Timeout.Infinite, 0);
        }

        // Lấy vị trí hiện tại và kiểm tra sự thay đổi
        private async Task UpdateLocation()
        {
            try
            {
                var location = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best));

                if (location != null && HasMoved(location))
                {
                    _lastLocation = location;
                    LocationChanged?.Invoke(this, location); // Gọi sự kiện khi có vị trí mới
                    await SendLocationUpdate(location); // Gửi vị trí cập nhật
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting location: {ex.Message}");
            }
        }

        // Kiểm tra xem vị trí có thay đổi đủ lớn hay không
        private bool HasMoved(Location location)
        {
            if (_lastLocation == null)
                return true;

            return Math.Abs(location.Latitude - _lastLocation.Latitude) > MovementThreshold ||
                   Math.Abs(location.Longitude - _lastLocation.Longitude) > MovementThreshold;
        }

        // Gửi vị trí cập nhật lên Master Server
        public async Task SendLocationUpdate(Location location)
        {
            if (_webSocketClient == null || _webSocketClient.State != WebSocketState.Open)
            {
                Console.WriteLine("WebSocket is not connected.");
                return;
            }

            try
            {
                var slaveInfo = new SlaveInfo
                {
                    ID = "Slave1", // ID duy nhất cho mỗi thiết bị slave
                    Name = "Slave Device 1", // Tên thiết bị
                    Latitude = location.Latitude,
                    Longitude = location.Longitude
                };

                // Chuyển đổi thông tin vị trí thành chuỗi JSON và gửi lên Master
                string locationMessage = JsonConvert.SerializeObject(slaveInfo);
                var bytes = Encoding.UTF8.GetBytes(locationMessage);
                await _webSocketClient.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
                Console.WriteLine($"Location sent: {locationMessage}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR sending location: {ex.Message}");
            }
        }
    }
}