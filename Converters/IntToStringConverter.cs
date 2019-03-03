using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace NodeGraph.Converters
{
	[ValueConversion( typeof( int ), typeof( string ) )]
	public class IntToStringConverter : IValueConverter
	{
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			return value.ToString();
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			return ( int )double.Parse( value as string );
		}
	}
}
