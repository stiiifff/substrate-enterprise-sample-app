using Parity.Substrate.EnterpriseSample.Services;
using Polkadot.Api;
using Prism.Navigation;
using System;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace Parity.Substrate.EnterpriseSample.ViewModels
{
    public class NodeLogsViewModel : BaseViewModel, INavigatedAware
    {
        private IDisposable logSubscription;

        public NodeLogsViewModel(INavigationService navigationService, ILightClient lightClient, IApplication polkadotApi)
            : base(navigationService, lightClient, polkadotApi)
        {
            Title = "Node logs";

            logSubscription = LightClient.Logs.Subscribe(log =>
                Device.BeginInvokeOnMainThread(() => Logs.Add(new LogRecord(log.Substring(0, 19), log.Substring(20))))
            );
        }

        public ObservableCollection<LogRecord> Logs { get; } = new ObservableCollection<LogRecord>();


        public override void OnNavigatedFrom(INavigationParameters parameters)
        {
            logSubscription.Dispose();
            logSubscription = null;
        }
    }

    public class LogRecord
    {
        public LogRecord(string timestamp, string message)
        {
            Timestamp = timestamp;
            Message = message;
        }

        public string Timestamp { get; set; }
        public string Message { get; set; }
    }
}
