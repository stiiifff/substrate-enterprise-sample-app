using Prism.Navigation;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Parity.Substrate.EnterpriseSample.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : TabbedPage
    {
        public MainPage()
        {
            InitializeComponent();
        }
        
        protected override void OnCurrentPageChanged()
        {
            base.OnCurrentPageChanged();
            ((CurrentPage as NavigationPage)?.CurrentPage?.BindingContext as INavigatedAware)?.OnNavigatedTo(new NavigationParameters());
        }
    }
}