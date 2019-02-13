using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace NodeGraph.View
{
	public class NodePropertyPortViewsContainer : ItemsControl
	{
		#region Properties

		public bool IsInput
		{
			get { return ( bool )GetValue( IsInputProperty ); }
			set { SetValue( IsInputProperty, value ); }
		}
		public static readonly DependencyProperty IsInputProperty =
			DependencyProperty.Register( "IsInput", typeof( bool ), typeof( NodePropertyPortViewsContainer ), new PropertyMetadata( false ) );

		#endregion // Properties

		#region Methods

		protected override DependencyObject GetContainerForItemOverride()
		{
			return new NodePropertyPortView( IsInput );
		}

		#endregion // Methods
	}
}
