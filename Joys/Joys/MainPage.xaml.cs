using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.WebSockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Essentials;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Joys.Models;
using Joys.Services;
using Joys.Services.Implementations;

namespace Joys
{
    public partial class MainPage : ContentPage
    {
        private readonly IMapService _mapService;

        private readonly IWebSocketClient _webSocketClient;

        private const string WebSocketUrl = "ws://192.168.0.127:49152/Echo";

        public MainPage()
        {
            InitializeComponent();

            _webSocketClient = WebSocketClient.Instance; // Sử dụng instance duy nhất của WebSocketClient

            // Khởi tạo instance duy nhất của MapService và thiết lập WebView
            _mapService = MapService.Instance;
            _mapService.Initialize(MapWebView); // Thiết lập WebView cho MapService
            _mapService.LoadOpenStreetMapAsync(); // Gọi hàm load bản đồ khi khởi tạo MainPage
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
    }
}