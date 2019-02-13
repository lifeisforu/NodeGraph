using NodeGraph.Model;
using NodeGraph.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NodeGraph.ViewModel
{
	public class FlowChartViewModel : ViewModelBase
	{
		#region Statics

		public static ObservableCollection<Type> NodeTypes = new ObservableCollection<Type>();

		public FlowChartView View;

		#endregion // Statics

		#region Properties

		private FlowChart _Model;
		public FlowChart Model
		{
			get { return _Model; }
			set
			{
				if( value != _Model )
				{
					_Model = value;
					RaisePropertyChanged( "Model" );
				}
			}
		}

		protected ObservableCollection<NodeViewModel> _NodeViewModels = new ObservableCollection<NodeViewModel>();
		public ObservableCollection<NodeViewModel> NodeViewModels
		{
			get { return _NodeViewModels; }
			set
			{
				if( value != _NodeViewModels )
				{
					RaisePropertyChanged( "NodeViewModels" );
				}
			}
		}

		protected ObservableCollection<ConnectorViewModel> _ConnectorViewModels = new ObservableCollection<ConnectorViewModel>();
		public ObservableCollection<ConnectorViewModel> ConnectorViewModels
		{
			get { return _ConnectorViewModels; }
			set
			{
				if( value != _ConnectorViewModels )
				{
					RaisePropertyChanged( "ConnectorViewModels" );
				}
			}
		}

		private Visibility _SelectionVisibility = Visibility.Collapsed;
		public Visibility SelectionVisibility
		{
			get { return _SelectionVisibility; }
			set
			{
				if( value != _SelectionVisibility )
				{
					_SelectionVisibility = value;
					RaisePropertyChanged( "SelectionVisibility" );
				}
			}
		}

		private double _SelectionStartX;
		public double SelectionStartX
		{
			get { return _SelectionStartX; }
			set
			{
				if( value != _SelectionStartX )
				{
					_SelectionStartX = value;
					RaisePropertyChanged( "SelectionStartX" );
				}
			}
		}

		private double _SelectionStartY;
		public double SelectionStartY
		{
			get { return _SelectionStartY; }
			set
			{
				if( value != _SelectionStartY )
				{
					_SelectionStartY = value;
					RaisePropertyChanged( "SelectionStartY" );
				}
			}
		}

		private double _SelectionWidth;
		public double SelectionWidth
		{
			get { return _SelectionWidth; }
			set
			{
				if( value != _SelectionWidth )
				{
					_SelectionWidth = value;
					RaisePropertyChanged( "SelectionWidth" );
				}
			}
		}

		private double _SelectionHeight;
		public double SelectionHeight
		{
			get { return _SelectionHeight; }
			set
			{
				if( value != _SelectionHeight )
				{
					_SelectionHeight = value;
					RaisePropertyChanged( "SelectionHeight" );
				}
			}
		}


		#endregion // Properties

		#region Constructor

		public FlowChartViewModel( FlowChart flowChart )
		{
			Model = flowChart;
		}

		#endregion // Constructor

		#region ContextMenu

		private Point _ContextMenuMouseLocation;
		public virtual void BuildContextMenuItems( ItemCollection items, Point mouseLocation )
		{
			items.Clear();

			_ContextMenuMouseLocation = mouseLocation;

			foreach( var nodeType in NodeTypes )
			{
				MenuItem menuItem = new MenuItem();

				var NodeAttrs = nodeType.GetCustomAttributes( typeof( NodeAttribute ), false ) as NodeAttribute[];
				if( 1 != NodeAttrs.Length )
					throw new ArgumentException( string.Format( "{0} must have NodeAttribute", nodeType.Name ) );

				menuItem.Header = "Create " + NodeAttrs[ 0 ].Header;
				menuItem.CommandParameter = nodeType;
				menuItem.Click += ContextMenuItem_Click;
				items.Add( menuItem );
			}
		}

		protected virtual void ContextMenuItem_Click( object sender, RoutedEventArgs e )
		{
			MenuItem menuItem = sender as MenuItem;
			Type nodeType = menuItem.CommandParameter as Type;

			Node node = NodeGraphManager.This.CreateNode( Guid.NewGuid(), Model, nodeType );
			node.X = _ContextMenuMouseLocation.X;
			node.Y = _ContextMenuMouseLocation.Y;
		}

		#endregion // ContextMenu
	}
}
