using System.Diagnostics;
using System.Windows.Input;
using Xamarin.Forms;

namespace Parity.Substrate.EnterpriseSample.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        public HomeViewModel()
        {
            Title = "Home";
            QueryChainStateCommand = new Command(QueryChainState);
        }

        public ICommand QueryChainStateCommand { get; }

        internal void LoadData()
        {
            IsBusy = true;
            try
            {
            }
            catch (System.Exception ex)
            {
                Trace.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void QueryChainState()
        {
            try
            {
            }
            catch (System.Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }
    }
}
