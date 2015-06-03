using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;

namespace InfNews
{
    class BoolVisibilityConverter : IValueConverter
    {
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			bool isVisible = (bool)value;

			if (isVisible)
			{
				return "Visible";
			}
			else
			{
				return "Collapsed";
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}
