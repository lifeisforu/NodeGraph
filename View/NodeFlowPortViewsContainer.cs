using NodeGraph.ViewModel;
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
	public class NodeFlowPortViewsContainer : ItemsControl
	{
		#region Properties

		public bool IsInput
		{
			get { return ( bool )GetValue( IsInputProperty ); }
			set { SetValue( IsInputProperty, value ); }
		}
		public static readonly DependencyProperty IsInputProperty =
			DependencyProperty.Register( "IsInput", typeof( bool ), typeof( NodeFlowPortViewsContainer ), new PropertyMetadata( false ) );

		#endregion // Properties

		#region Overrides ItemsControl

		protected override void PrepareContainerForItemOverride( DependencyObject element, object item )
		{
			base.PrepareContainerForItemOverride( element, item );

			var attrs = item.GetType().GetCustomAttributes( typeof( NodeFlowPortViewModelAttribute ), false ) as NodeFlowPortViewModelAttribute[];
			if( 1 != attrs.Length )
				throw new Exception( "A NodeFlowPortViewModelAttribute must exist for NodeFlowPortViewModel class" );

			FrameworkElement fe = element as FrameworkElement;

			ResourceDictionary resourceDictionary = new ResourceDictionary
			{
				Source = new Uri( "/NodeGraph;component/Themes/generic.xaml", UriKind.RelativeOrAbsolute )
			};

			Style style = resourceDictionary[ attrs[ 0 ].ViewStyleName ] as Style;
			if( null == style )
			{
				style = Application.Current.TryFindResource( attrs[ 0 ].ViewStyleName ) as Style;
			}
			fe.Style = style;

			if( null == fe.Style )
				throw new Exception( String.Format( "{0} does not exist", attrs[ 0 ].ViewStyleName ) );
		}

		protected override DependencyObject GetContainerForItemOverride()
		{
			return new NodeFlowPortView( IsInput );
		}

		#endregion // Overrides ItemsControl
	}
}
