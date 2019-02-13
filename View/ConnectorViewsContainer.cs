using NodeGraph.View;
using System.Windows;
using System.Windows.Controls;

namespace NodeGraph.View
{
	public class ConnectorViewsContainer : ItemsControl
	{
		protected override DependencyObject GetContainerForItemOverride()
		{
			return new ConnectorView();
		}
	}
}