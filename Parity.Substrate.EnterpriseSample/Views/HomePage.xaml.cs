using Parity.Substrate.EnterpriseSample.ViewModels;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Parity.Substrate.EnterpriseSample.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HomePage : ContentPage
    {
        public HomePage()
        {
            InitializeComponent();
        }

        internal async Task OnNavigatedTo()
        {
            if (BindingContext is HomeViewModel vm)
                await Task.Run(() => vm.LoadData());
        }
    }
}