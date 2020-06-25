
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
            if (BindingContext is AboutViewModel vm)
                vm.SetShowNodeLogsPage(async () => await Navigation.PushModalAsync(
                    new NavigationPage(new NodeLogsPage())));
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
        }

        internal async Task OnNavigatedTo()
        {
            if (BindingContext is AboutViewModel vm)
                await Task.Run(() => vm.LoadData());
        }
    }
}
