using System;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

class Program
{
    private static async Task StartMultipleClients(int numberOfClients)
    {
        // Danh sách các task
        List<Task> clientTasks = new List<Task>();

        // Tạo và khởi chạy các clients dựa trên số lượng input
        for (int i = 1; i <= numberOfClients; i++)
        {
            string clientName = $"Client{i}";
            clientTasks.Add(StartClient(clientName));
        }

        // Đợi tất cả các task hoàn thành
        await Task.WhenAll(clientTasks);
    }

    private static async Task Main(string[] args)
    {
        await StartMultipleClients(100);
    }

    static async Task StartClient(string clientName)
    {

        using (ClientWebSocket webSocket = new ClientWebSocket())
        {
            try
            {
                // Cập nhật địa chỉ IP và cổng của server WebSocket, thêm đường dẫn "/Echo"
                Uri serverUri = new Uri("ws://192.168.1.13:49152/Echo"); // Địa chỉ IP và cổng của server, thêm đường dẫn "/Echo"
                // Kết nối tới server
                await webSocket.ConnectAsync(serverUri, CancellationToken.None);
                Console.WriteLine($"{clientName} connected!");

                Random random = new Random();
                // Gửi 100 requests liên tục với mô phỏng giao dịch mua/bán
                for (int i = 1; i <= 90000000; i++)
                {
                    // Random action: Mua (buy) hoặc Bán (sell)
                    string action = random.Next(2) == 0 ? "buy" : "sell";
                    double amount = Math.Round(random.NextDouble() * 10, 2); // Giao dịch số lượng Bitcoin ngẫu nhiên (0.01 -> 10 BTC)

                    var transaction = new BitcoinTransaction
                    {
                        ClientID = $"{clientName}",
                        Action = action,
                        Amount = amount,
                        Price = 20000 + random.NextDouble() * 5000 // Giá Bitcoin ngẫu nhiên trong khoảng 20000 -> 25000 USD
                    };

                    // Chuyển đổi transaction thành JSON
                    string message = JsonConvert.SerializeObject(transaction);

                    // Gửi JSON đến server
                    await SendMessage(webSocket, message);

                    // Nhận phản hồi từ server
                    await ReceiveMessage(webSocket);

                    // Thêm độ trễ để dễ dàng quan sát quá trình (có thể điều chỉnh)
                    await Task.Delay(1000);  // Đảm bảo độ trễ để tránh spam requests quá nhanh
                }

                // Đóng kết nối
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                Console.WriteLine($"{clientName} disconnected!");
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"SocketException: {ex.Message}");
            }
            catch (WebSocketException wsEx)
            {
                Console.WriteLine($"WebSocketException: {wsEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }
    }

    static async Task SendMessage(ClientWebSocket webSocket, string message)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(message);
        await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        Console.WriteLine($"Sent: {message}");  // Log để đảm bảo rằng tin nhắn được gửi
    }

    static async Task ReceiveMessage(ClientWebSocket webSocket)
    {
        try
        {
            byte[] buffer = new byte[1024];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            Console.WriteLine($"Received: {message}");  // Log phản hồi nhận được từ server
        }
        catch (WebSocketException ex)
        {
            Console.WriteLine($"WebSocketException while receiving: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception while receiving: {ex.Message}");
        }
    }
}

// Mô hình giao dịch Bitcoin
public class BitcoinTransaction
{
    public string ClientID { get; set; }
    public string Action { get; set; }  // "buy" or "sell"
    public double Amount { get; set; }  // Số lượng Bitcoin
    public double Price { get; set; }   // Giá của Bitcoin tại thời điểm giao dịch
}
