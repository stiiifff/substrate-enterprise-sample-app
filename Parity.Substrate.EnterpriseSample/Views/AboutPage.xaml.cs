
using Parity.Substrate.EnterpriseSample.ViewModels;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Parity.Substrate.EnterpriseSample.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            if (BindingContext is AboutViewModel vm)
            {
                await Task.Run(() => vm.LoadData());
            }
        }
    }
}
