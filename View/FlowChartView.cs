using NodeGraph.Model;
using NodeGraph.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NodeGraph.View
{
	[TemplatePart( Name = "PART_NodeCanvas", Type = typeof( FrameworkElement ) )]
	[TemplatePart( Name = "PART_DragAndSelectionCanvas", Type = typeof( FrameworkElement ) )]
	public class FlowChartView : ContentControl
	{
		#region Fields

		protected FlowChartViewModel _ViewModel;

		#endregion // Fields

		#region Properties

		protected FrameworkElement _PartNodeViewsContainer;
		public FrameworkElement PartNodeCanvas
		{
			get { return _PartNodeViewsContainer; }
		}

		protected FrameworkElement _PartDragAndSelectionCanvas;
		public FrameworkElement PartDragAndSelectionCanvas
		{
			get { return _PartDragAndSelectionCanvas; }
		}

		#endregion // Properties

		#region Constructors

		static FlowChartView()
		{
			DefaultStyleKeyProperty.OverrideMetadata( typeof( FlowChartView ), new FrameworkPropertyMetadata( typeof( FlowChartView ) ) );
		}

		public FlowChartView()
		{
			Focusable = true;
			DataContextChanged += FlowChartView_DataContextChanged;

			ContextMenu = new ContextMenu();
			ContextMenuOpening += FlowChartView_ContextMenuOpening;
		}

		private void FlowChartView_DataContextChanged( object sender, DependencyPropertyChangedEventArgs e )
		{
			_ViewModel = DataContext as FlowChartViewModel;
			if( null == _ViewModel )
				throw new Exception( "ViewModel must be bound as DataContext in FlowChartView." );
			_ViewModel.View = this;
		}

		#endregion // Constructors

		#region Template

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_PartNodeViewsContainer = GetTemplateChild( "PART_NodeViewsContainer" ) as FrameworkElement;
			if( null == _PartNodeViewsContainer )
				throw new Exception( "PART_NodeViewsContainer can not be null in FlowChartView" );

			_PartDragAndSelectionCanvas = GetTemplateChild( "PART_DragAndSelectionCanvas" ) as FrameworkElement;
			if( null == _PartDragAndSelectionCanvas )
				throw new Exception( "PART_DragAndSelectionCanvas can not be null in FlowChartView" );
		}

		#endregion // Template

		#region Mouse Events

		protected override void OnMouseLeftButtonDown( MouseButtonEventArgs e )
		{
			base.OnMouseLeftButtonDown( e );

			if( null == _ViewModel )
			{
				return;
			}

			Keyboard.Focus( this );

			if( !NodeGraphManager.This.IsNodeDragging &&
				!NodeGraphManager.This.IsConnecting &&
				!NodeGraphManager.This.IsSelecting )
			{
				bool bCtrl = Keyboard.IsKeyDown( Key.LeftCtrl );
				bool bShift = Keyboard.IsKeyDown( Key.LeftShift );
				bool bAlt = Keyboard.IsKeyDown( Key.LeftAlt );

				if( !bCtrl && !bShift && !bAlt )
				{
					NodeGraphManager.This.DeslectAllNodes( _ViewModel.Model );
				}

				NodeGraphManager.This.StartDragSelection( _ViewModel.Model, e.GetPosition( this ) );
			}
		}

		protected override void OnMouseMove( MouseEventArgs e )
		{
			base.OnMouseMove( e );

			if( null == _ViewModel )
			{
				return;
			}

			if( NodeGraphManager.This.IsConnecting )
			{
				NodeGraphManager.This.UpdateConnection();
			}
			else if( NodeGraphManager.This.IsNodeDragging )
			{
				NodeGraphManager.This.DragNode( e.GetPosition( this ) );
			}
			else if( NodeGraphManager.This.IsSelecting )
			{
				Point position = e.GetPosition( this );

				// gather nodes in area.

				bool bCtrl = Keyboard.IsKeyDown( Key.LeftCtrl );
				bool bShift = Keyboard.IsKeyDown( Key.LeftShift );
				bool bAlt = Keyboard.IsKeyDown( Key.LeftAlt );

				NodeGraphManager.This.UpdateDragSelection( position, bCtrl, bShift, bAlt );
			}
		}

		protected override void OnMouseLeave( MouseEventArgs e )
		{
			base.OnMouseLeave( e );

			if( null == _ViewModel )
			{
				return;
			}

			NodeGraphManager.This.EndConnection();
			NodeGraphManager.This.EndDragNode();
			NodeGraphManager.This.EndDragSelection( true );
		}

		protected override void OnMouseLeftButtonUp( MouseButtonEventArgs e )
		{
			base.OnMouseLeftButtonUp( e );

			if( null == _ViewModel )
			{
				return;
			}

			if( NodeGraphManager.This.IsConnecting )
			{
				Connector connector = NodeGraphManager.This.ConnectingConnector;
				if( ( null == connector.StartPort ) || ( null == connector.EndPort ) )
				{
					NodePort firstPort = NodeGraphManager.This.FirstConnectionPort;

					NodeGraphManager.This.EndConnection();

					Point mouseLocation = Mouse.GetPosition( this );

					HitTestResult hitResult = VisualTreeHelper.HitTest( this, mouseLocation );
					if( ( null != hitResult ) && ( null != hitResult.VisualHit ) )
					{
						FrameworkElement element = hitResult.VisualHit as FrameworkElement;
						if( null == ViewUtil.FindFirstParent<NodeView>( element ) )
						{
							Node node = NodeGraphManager.This.CreateRouterNodeForPort(
								Guid.NewGuid(), _ViewModel.Model, firstPort, mouseLocation.X, mouseLocation.Y, 0 );
						}
					}
				}
				else
				{
					NodeGraphManager.This.EndConnection();
				}
			}
			NodeGraphManager.This.EndDragNode();
			NodeGraphManager.This.EndDragSelection( false );
		}

		protected override void OnLostFocus( RoutedEventArgs e )
		{
			base.OnLostFocus( e );

			if( null == _ViewModel )
			{
				return;
			}

			NodeGraphManager.This.EndConnection();
			NodeGraphManager.This.EndDragNode();
			NodeGraphManager.This.EndDragSelection( true );
		}

		protected override void OnMouseRightButtonUp( MouseButtonEventArgs e )
		{
			base.OnMouseRightButtonUp( e );

			if( null == _ViewModel )
			{
				return;
			}
		}

		protected override void OnKeyDown( KeyEventArgs e )
		{
			base.OnKeyDown( e );

			if( null == _ViewModel )
			{
				return;
			}

			if( Key.Delete == e.Key )
			{
				NodeGraphManager.This.DestroySelectedNodes( _ViewModel.Model );
			}
			else if( Key.Escape == e.Key )
			{
				NodeGraphManager.This.DeslectAllNodes( _ViewModel.Model );
			}
			else if( Key.A == e.Key )
			{
				if( Keyboard.IsKeyDown( Key.LeftCtrl ) )
				{
					NodeGraphManager.This.SelectAllNodes( _ViewModel.Model );
				}
			}
		}

		#endregion // Mouse Events

		#region Context Menu

		private void FlowChartView_ContextMenuOpening( object sender, ContextMenuEventArgs e )
		{
			if( ( null == _ViewModel ) || !FlowChartViewModel.ContextMenuEnabled )
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

		#endregion // Context Menu
	}
}
