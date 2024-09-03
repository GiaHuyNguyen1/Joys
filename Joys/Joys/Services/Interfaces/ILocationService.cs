using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Joys.Services
{
    public interface ILocationService
    {
        void StartTrackingLocation();

        void StopTrackingLocation();

        event EventHandler<Location> LocationChanged;

        Task SendLocationUpdate(Location location);
    }

}
