using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace Joys.Services
{
    public interface IWebSocketClient
    {
        // Kết nối tới WebSocket server
        Task ConnectAsync(string uri);

        // Ngắt kết nối khỏi WebSocket server
        Task DisconnectAsync();

        // Gửi vị trí tới server
        Task SendLocationAsync();

        // Hiển thị thông báo lên màn hình hoặc log
        void WriteToScreen(string message);

        Task SendMessageAsync(string message);
    }
}
