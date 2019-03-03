using PropertyTools.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NodeGraph.Controls
{
	public class NumericTextBox : TextBoxEx
	{
		#region Properties

		public bool IsInteger
		{
			get { return ( bool )GetValue( IsIntegerProperty ); }
			set { SetValue( IsIntegerProperty, value ); }
		}
		public static readonly DependencyProperty IsIntegerProperty =
			DependencyProperty.Register( "IsInteger", typeof( bool ), typeof( NumericTextBox ), new PropertyMetadata( true ) );

		#endregion // Properties

		#region Events

		protected override void OnPreviewTextInput( TextCompositionEventArgs e )
		{
			Regex regex = IsInteger ? new Regex( "[0-9]" ) : new Regex( "[0-9.]" );
			e.Handled = !regex.IsMatch( e.Text );
		}

		protected override void OnTextInput( TextCompositionEventArgs e )
		{
			base.OnTextInput( e );
		}

		protected override void OnTextChanged( TextChangedEventArgs e )
		{
			Regex regex = new Regex( "^[0-9]+[.]?[0-9]*$" );
			if( !IsInteger && !regex.IsMatch( Text ) )
			{
				string[] tokens = Text.Split( '.' );
				string newText = "";
				for( int i = 0; i < tokens.Length; ++i )
				{
					newText += tokens[ i ];
					if( ( 0 == i ) && ( 1 < tokens.Length ) )
					{
						newText += ".";
					}
				}
				Text = newText;
			}
		}

		#endregion // Events
	}
}
