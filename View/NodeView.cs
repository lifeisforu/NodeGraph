using NodeGraph.Model;
using NodeGraph.ViewModel;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace NodeGraph.View
{
	public class NodeView : ContentControl
	{
		#region Fields

		protected NodeViewModel _ViewModel;

		#endregion // Fields

		#region Border Properties

		public bool IsSelected
		{
			get { return ( bool )GetValue( IsSelectedProperty ); }
			set { SetValue( IsSelectedProperty, value ); }
		}
		public static readonly DependencyProperty IsSelectedProperty =
			DependencyProperty.Register( "IsSelected", typeof( bool ), typeof( NodeView ), new PropertyMetadata( false ) );

		#endregion // Border Properties

		#region Properties

		public bool HasConnection
		{
			get { return ( bool )GetValue( HasConnectionProperty ); }
			set { SetValue( HasConnectionProperty, value ); }
		}
		public static readonly DependencyProperty HasConnectionProperty =
			DependencyProperty.Register( "HasConnection", typeof( bool ), typeof( NodeView ), new PropertyMetadata( false ) );

		#endregion // Properties

		#region Constructors

		public NodeView()
		{
			DataContextChanged += NodeView_DataContextChanged;
			Loaded += NodeView_Loaded;
			Unloaded += NodeView_Unloaded;

			ContextMenu = new ContextMenu();
			ContextMenuOpening += NodeView_ContextMenuOpening;
		}

		private void NodeView_Loaded( object sender, RoutedEventArgs e )
		{
			FlowChart flowChart = _ViewModel.Model.Owner;
			NodeGraphManager.UpdateContentSize( flowChart );
		}

		private void NodeView_Unloaded( object sender, RoutedEventArgs e )
		{
			FlowChart flowChart = _ViewModel.Model.Owner;
			NodeGraphManager.UpdateContentSize( flowChart );
		}

		private void NodeView_DataContextChanged( object sender, DependencyPropertyChangedEventArgs e )
		{
			_ViewModel = DataContext as NodeViewModel;
			if( null == _ViewModel )
				throw new Exception( "ViewModel must be bound as DataContext in NodeView." );
			_ViewModel.View = this;
		}

		#endregion // Constructors

		#region Template Events

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
		}

		#endregion // Template Events

		#region Mouse Events

		protected override void OnMouseLeftButtonUp( MouseButtonEventArgs e )
		{
			base.OnMouseLeftButtonUp( e );

			FlowChart flowChart = _ViewModel.Model.Owner;

			NodeGraphManager.EndConnection();
			NodeGraphManager.EndDragSelection( false );

			if( !NodeGraphManager.AreNodesReallyDragged &&
				NodeGraphManager.MouseLeftDownNode == _ViewModel.Model )
			{
				NodeGraphManager.TrySelection( flowChart, _ViewModel.Model,
					Keyboard.IsKeyDown( Key.LeftCtrl ),
					Keyboard.IsKeyDown( Key.LeftShift ),
					Keyboard.IsKeyDown( Key.LeftAlt ) );
			}

			NodeGraphManager.EndDragNode();

			NodeGraphManager.MouseLeftDownNode = null;

			e.Handled = true;
		}

		protected override void OnMouseLeftButtonDown( MouseButtonEventArgs e )
		{
			base.OnMouseLeftButtonDown( e );

			FlowChart flowChart = _ViewModel.Model.Owner;
			FlowChartView flowChartView = flowChart.ViewModel.View;
			Keyboard.Focus( flowChartView );

			NodeGraphManager.EndConnection();
			NodeGraphManager.EndDragNode();
			NodeGraphManager.EndDragSelection( false );

			NodeGraphManager.MouseLeftDownNode = _ViewModel.Model;

			NodeGraphManager.BeginDragNode( flowChart );

			e.Handled = true;
		}

		protected override void OnMouseRightButtonDown( MouseButtonEventArgs e )
		{
			base.OnMouseRightButtonDown( e );

			e.Handled = true;
		}

		protected override void OnMouseRightButtonUp( MouseButtonEventArgs e )
		{
			base.OnMouseRightButtonUp( e );

			e.Handled = true;
		}

		protected override void OnMouseMove( MouseEventArgs e )
		{
			base.OnMouseMove( e );

			if( NodeGraphManager.IsNodeDragging &&
				( NodeGraphManager.MouseLeftDownNode == _ViewModel.Model ) &&
				!IsSelected )
			{
				NodeGraphManager.TrySelection( _ViewModel.Model.Owner, _ViewModel.Model, false, false, false );
			}
		}

		#endregion // Mouse Events

		#region Selection

		public void OnSelectionChanged( bool isSelected )
		{
			IsSelected = isSelected;
		}

		#endregion // Selection
		
		#region Connection

		public virtual void OnPortConnectionChanged()
		{
			HasConnection = ( 0 < _ViewModel.InputFlowPortViewModels.Count ) ||
				( 0 < _ViewModel.OutputFlowPortViewModels.Count ) ||
				( 0 < _ViewModel.InputPropertyPortViewModels.Count ) ||
				( 0 < _ViewModel.OutputPropertyPortViewModels.Count );
		}

		#endregion // Connection

		#region ContextMenu

		private void NodeView_ContextMenuOpening( object sender, ContextMenuEventArgs e )
		{
			if( ( null == _ViewModel ) || !NodeViewModel.ContextMenuEnabled )
			{
				e.Handled = true;
				return;
			}

			ContextMenu contextMenu = new ContextMenu();
			contextMenu.PlacementTarget = this;
			contextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Left;
			BuildContextMenuEventArgs args = new BuildContextMenuEventArgs(
				contextMenu, Mouse.GetPosition( this ) );
			_ViewModel.InvokeBuildContextMenuEvent( args );

			if( 0 == contextMenu.Items.Count )
			{
				ContextMenu = null;
				e.Handled = true;
			}
			else
			{
				ContextMenu = contextMenu;
			}
		}

		#endregion // ContextMenu
	}
}
