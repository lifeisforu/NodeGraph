using NodeGraph.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace NodeGraph.View
{
	public class ViewUtil
	{
		public static T FindFirstParent<T>( DependencyObject child ) where T : DependencyObject
		{
			DependencyObject parent = VisualTreeHelper.GetParent( child );

			if( ( null == parent ) || parent is T )
				return parent as T;

			return FindFirstParent<T>( parent );
		}

		public static T FindFirstParent<T>( DependencyObject child, string parentName ) where T : DependencyObject
		{
			DependencyObject parent = VisualTreeHelper.GetParent( child );

			T parentAsT = parent as T;
			if( ( null != parentAsT ) && ( ( parentAsT as FrameworkElement ).Name == parentName ) )
				return parentAsT;

			return FindFirstParent<T>( parent );
		}

		public static void FindChildren<T>( DependencyObject parent, List< T > outChildren  ) where T : DependencyObject
		{
			for( int i = 0; i < VisualTreeHelper.GetChildrenCount( parent ); i++ )
			{
				var child = VisualTreeHelper.GetChild( parent, i );
				if( child is T )
					outChildren.Add( child as T );
				FindChildren<T>(child, outChildren);
			}
		}

		// https://stackoverflow.com/questions/636383/how-can-i-find-wpf-controls-by-name-or-type
		public static T FindChild<T>( DependencyObject parent, string childName = null ) where T : DependencyObject
		{
			// Confirm parent and childName are valid. 
			if( parent == null ) return null;

			T foundChild = null;

			int childrenCount = VisualTreeHelper.GetChildrenCount( parent );
			for( int i = 0; i < childrenCount; i++ )
			{
				var child = VisualTreeHelper.GetChild( parent, i );
				// If the child is not of the request child type child
				T childType = child as T;
				if( childType == null )
				{
					// recursively drill down the tree
					foundChild = FindChild<T>( child, childName );

					// If the child is found, break so we do not overwrite the found child. 
					if( foundChild != null ) break;
				}
				else if( !string.IsNullOrEmpty( childName ) )
				{
					var frameworkElement = child as FrameworkElement;
					// If the child's name is set for search
					if( frameworkElement != null && frameworkElement.Name == childName )
					{
						// if the child's name is of the request name
						foundChild = ( T )child;
						break;
					}
				}
				else
				{
					// child element found.
					foundChild = ( T )child;
					break;
				}
			}

			return foundChild;
		}

		public static Point GetRelativeCenterLocation( FrameworkElement visual, FrameworkElement parentVisual )
		{
			return visual.TransformToAncestor( parentVisual ).Transform(
				new Point( visual.ActualWidth / 2.0, visual.ActualHeight / 2.0 ) );
		}

		public static NodeView FindNodeView( FrameworkElement rootVisual, Guid guid )
		{
			List<NodeView> nodeViews = new List<NodeView>();
			FindChildren( rootVisual, nodeViews );

			foreach( var nodeView in nodeViews )
			{
				NodeViewModel nodeVM = nodeView.DataContext as NodeViewModel;
				if( nodeVM.Model.Guid == guid )
					return nodeView;
			}

			return null;
		}

		public static NodePortView FindNodePortView<T>( FrameworkElement rootVisual, Guid guid, string portName ) where T : NodePortView
		{
			NodeView nodeView = FindNodeView( rootVisual, guid );

			List<T> portViews = new List<T>();
			FindChildren( nodeView, portViews );

			foreach( var portView in portViews )
			{
				NodePortViewModel portVM = portView.DataContext as NodePortViewModel;
				if( portVM.Model.Name == portName )
					return portView;
			}

			return null;
		}

		public static ConnectorView FindConnectorView( FrameworkElement rootVisual, Guid guid )
		{
			List<ConnectorView> ConnectorViews = new List<ConnectorView>();
			FindChildren( rootVisual, ConnectorViews );

			foreach( var ConnectorView in ConnectorViews )
			{
				ConnectorViewModel connectionVM = ConnectorView.DataContext as ConnectorViewModel;
				if( connectionVM.Model.Guid == guid )
					return ConnectorView;
			}

			return null;
		}
	}
}
