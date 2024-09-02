using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Joys.Services
{
    public interface ILocationService
    {
        // Bắt đầu theo dõi vị trí của Slave
        void StartTrackingLocation();

        // Gửi vị trí hiện tại của Slave đến Master
        Task SendLocationUpdate(Location location);

        // Sự kiện để theo dõi khi vị trí thay đổi
        event EventHandler<Location> LocationChanged;
    }
}
