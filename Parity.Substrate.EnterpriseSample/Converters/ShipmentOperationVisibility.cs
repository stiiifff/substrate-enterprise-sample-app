using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using Xamarin.Forms;

namespace Parity.Substrate.EnterpriseSample.Converters
{
    public class ShipmentOperationVisibility : IValueConverter
    {
        Dictionary<string, string[]> allowedOperationsPerStatus = new Dictionary<string, string[]>
        {
            {"Pending", new [] {"Pickup"}},
            {"InTransit", new []{"Scan","Deliver"}},
            {"Delivered", new string[0]}
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = (string)value ?? "";
            var operation = (string)parameter;

            return allowedOperationsPerStatus.ContainsKey(status) &&
                allowedOperationsPerStatus[status].Contains(operation);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
