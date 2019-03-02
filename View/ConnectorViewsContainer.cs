using ConnectorGraph.ViewModel;
using NodeGraph.View;
using NodeGraph.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;

namespace NodeGraph.View
{
	public class ConnectorViewsContainer : ItemsControl
	{
		protected override void PrepareContainerForItemOverride( DependencyObject element, object item )
		{
			base.PrepareContainerForItemOverride( element, item );

			var attrs = item.GetType().GetCustomAttributes( typeof( ConnectorViewModelAttribute ), false ) as ConnectorViewModelAttribute[];
			if( 1 != attrs.Length )
				throw new Exception( "A ConnectorViewModelAttribute must exist for ConnectorViewModel class" );

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
			return new ConnectorView();
		}
	}
}