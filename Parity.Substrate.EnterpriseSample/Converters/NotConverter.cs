using System;
using Xamarin.Forms;

namespace Parity.Substrate.EnterpriseSample.Converters
{
	public class NotConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value == null)
				return true;
			if (value.GetType() != typeof(bool))
				return true;
			return !((bool)value);
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
