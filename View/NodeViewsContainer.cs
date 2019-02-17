using NodeGraph.Model;
using NodeGraph.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NodeGraph.View
{
	public class NodeViewsContainer : ItemsControl
    {
		#region Overrides ItemsControl

		protected override void PrepareContainerForItemOverride( DependencyObject element, object item )
		{
			base.PrepareContainerForItemOverride( element, item );

			var attrs = item.GetType().GetCustomAttributes( typeof( NodeViewModelAttribute ), false ) as NodeViewModelAttribute[];
			if( 1 != attrs.Length )
				throw new Exception( "A NodeViewModelAttribute must exist for NodeViewModel class" );

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
			return new NodeView();
		}

		#endregion // Overrides ItemsControl
	}
}
