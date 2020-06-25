using System;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace Parity.Substrate.EnterpriseSample.ViewModels
{
    public class NodeLogsViewModel : BaseViewModel
    {
        public NodeLogsViewModel()
        {
            Title = "Node logs";
            Logs = new ObservableCollection<LogRecord>();

            var _subscription = LightClient.Logs.Subscribe(log =>
                Device.BeginInvokeOnMainThread(() => Logs.Add(new LogRecord(log.Substring(0, 19), log.Substring(20))))
            );
        }

        public ObservableCollection<LogRecord> Logs { get; }
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
