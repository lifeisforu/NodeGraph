using NodeGraph.Model;
using NodeGraph.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
