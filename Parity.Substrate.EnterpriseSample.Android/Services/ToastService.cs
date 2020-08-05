using Android.Widget;
using Xamarin.Forms;
using Application = Android.App.Application;

namespace Parity.Substrate.EnterpriseSample.Services
{
    public class ToastService : IToastService
    {
        public void ShowShortToast(string message)
        {
            Device.BeginInvokeOnMainThread(() => Toast.MakeText(Application.Context, message, ToastLength.Short).Show());
        }

        public void ShowLongToast(string message)
        {
            Device.BeginInvokeOnMainThread(() => Toast.MakeText(Application.Context, message, ToastLength.Long).Show());
        }
    }
}
