using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Xamarin.Essentials;
using Android.Locations;
using Android.Content;

namespace Joys.Droid
{
    [Activity(Label = "Joys", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize )]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());

            // Yêu cầu quyền truy cập vị trí khi khởi động ứng dụng
            var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                Console.WriteLine("Location permission not granted.");
            }

            // Kiểm tra nếu GPS chưa bật, yêu cầu người dùng bật GPS
            CheckIfGpsEnabled();

        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private void CheckIfGpsEnabled()
        {
            LocationManager locationManager = (LocationManager)GetSystemService(Context.LocationService);
            bool isGpsEnabled = locationManager.IsProviderEnabled(LocationManager.GpsProvider);

            if (!isGpsEnabled)
            {
                // Thông báo cho người dùng bật GPS
                ShowGpsEnableDialog();
            }
        }

        // Hiển thị thông báo yêu cầu bật GPS
        private void ShowGpsEnableDialog()
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetTitle("GPS Disabled")
                   .SetMessage("GPS is disabled. Please enable GPS to allow location tracking.")
                   .SetPositiveButton("Enable GPS", (sender, e) =>
                   {
                       Intent intent = new Intent(Android.Provider.Settings.ActionLocationSourceSettings);
                       StartActivity(intent);
                   })
                   .SetNegativeButton("Cancel", (sender, e) => { })
                   .Show();
        }
    }
}