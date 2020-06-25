using Parity.Substrate.EnterpriseSample.ViewModels;
using Prism.Navigation;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Parity.Substrate.EnterpriseSample.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NodeLogsPage : ContentPage
    {
        public NodeLogsPage()
        {
            InitializeComponent();
        }

        protected override bool OnBackButtonPressed()
        {
            (BindingContext as NodeLogsViewModel)?.OnNavigatedFrom(new NavigationParameters());
            return false;
        }
    }
}