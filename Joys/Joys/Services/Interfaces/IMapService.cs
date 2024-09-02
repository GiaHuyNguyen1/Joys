using System.Threading.Tasks;
using Xamarin.Forms;

namespace Joys.Services
{
    public interface IMapService
    {
        // Hàm để load bản đồ với vị trí hiện tại
        Task LoadOpenStreetMapAsync();

        // Hàm để thêm marker vào bản đồ (nếu cần thêm nhiều marker sau này)
        Task AddMarkerAsync(double latitude, double longitude, string name);

        void Initialize(WebView mapWebView);
    }
}
