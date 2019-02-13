using System.Windows;
using System.Windows.Controls;

namespace NodeGraph.View
{
	public class NodeViewsContainer : ItemsControl
    {
		protected override DependencyObject GetContainerForItemOverride()
		{
			return new NodeView();
		}
	}
}
