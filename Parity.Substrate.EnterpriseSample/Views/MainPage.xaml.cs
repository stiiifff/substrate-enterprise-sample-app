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

        App App => ((App) Application.Current);

        override protected async void OnCurrentPageChanged()
        {
            if (CurrentPage is NavigationPage navpage)
            {
                //if (navpage.CurrentPage is HomePage _home)
                //{
                //    if (!App.IsPolkadotApiConnected)
                //        App.ConnectToNode();
                //}

                if (navpage.CurrentPage is AboutPage about)
                    await about.OnNavigatedTo();
            }
                
        }
    }
}