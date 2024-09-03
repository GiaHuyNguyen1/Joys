using Joys.Models;
using Newtonsoft.Json;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Xamarin.Essentials;

namespace Joys.Services
{
    public class WebSocketClient : IWebSocketClient
    {
        private ClientWebSocket _webSocket;
        private static readonly Lazy<WebSocketClient> _instance = new Lazy<WebSocketClient>(() => new WebSocketClient());

        // Private constructor để ngăn không cho tạo instance từ bên ngoài class
        private WebSocketClient()
        {
        }

        // Instance duy nhất của WebSocketClient
        public static WebSocketClient Instance => _instance.Value;

        // Kết nối tới WebSocket server
        public async Task ConnectAsync(string uri)
        {
            _webSocket = new ClientWebSocket();
            try
            {
                await _webSocket.ConnectAsync(new Uri(uri), CancellationToken.None);
                WriteToScreen("CONNECTED to WebSocket server.");
                await ReceiveMessagesAsync(); // Bắt đầu nhận thông điệp từ server
            }
            catch (Exception ex)
            {
                WriteToScreen($"ERROR: {ex.Message}");
            }
        }

        // Ngắt kết nối khỏi WebSocket server
        public async Task DisconnectAsync()
        {
            if (_webSocket != null && _webSocket.State == WebSocketState.Open)
            {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by user", CancellationToken.None);
                WriteToScreen("DISCONNECTED from WebSocket server.");
            }
        }

        // Gửi vị trí tới server
        public async Task SendLocationAsync()
        {
            if (_webSocket == null || _webSocket.State != WebSocketState.Open)
            {
                WriteToScreen("WebSocket is not connected.");
                return;
            }

            try
            {
                var location = await Geolocation.GetLastKnownLocationAsync();
                if (location != null)
                {
                    var slaveInfo = new SlaveInfo
                    {
                        ID = "Slave2", // Thay đổi ID này cho mỗi thiết bị slave
                        Name = "Slave Device 2", // Tên thiết bị
                        Latitude = location.Latitude,
                        Longitude = location.Longitude
                    };

                    string locationMessage = JsonConvert.SerializeObject(slaveInfo);
                    var bytes = Encoding.UTF8.GetBytes(locationMessage);
                    await _webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
                    WriteToScreen($"Location sent: {locationMessage}");
                }
                else
                {
                    WriteToScreen("Unable to get location.");
                }
            }
            catch (Exception ex)
            {
                WriteToScreen($"ERROR sending location: {ex.Message}");
            }
        }

        // Hiển thị thông báo lên màn hình hoặc log
        public void WriteToScreen(string message)
        {
            // Bạn có thể tùy chỉnh cách hiển thị thông báo, ví dụ: log ra màn hình console hoặc giao diện người dùng
            Console.WriteLine(message);
        }

        // Hàm nhận tin nhắn từ server
        private async Task ReceiveMessagesAsync()
        {
            var buffer = new byte[1024];
            try
            {
                while (_webSocket.State == WebSocketState.Open)
                {
                    var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by server", CancellationToken.None);
                        WriteToScreen("DISCONNECTED by server.");
                    }
                    else
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        WriteToScreen($"Received from server: {message}");
                    }
                }
            }
            catch (WebSocketException ex)
            {
                WriteToScreen($"ERROR: Connection closed unexpectedly. {ex.Message}");
            }
        }

        public async Task SendMessageAsync(string message)
        {
            if (_webSocket != null && _webSocket.State == WebSocketState.Open)
            {
                var buffer = Encoding.UTF8.GetBytes(message);
                await _webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
}
