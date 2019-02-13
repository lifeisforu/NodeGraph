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
	[TemplatePart( Name = "PART_ContextMenu", Type = typeof( ContextMenu ) )]
	[TemplatePart( Name = "PART_DragAndSelectionCanvas", Type = typeof( Canvas ) )]
	public class FlowChartView : ContentControl
	{
		#region Fields

		protected FlowChartViewModel _ViewModel;

		#endregion // Fields

		#region Properties

		protected ConnectorViewsContainer _PartConnectorViewsContainer;
		public ConnectorViewsContainer PartConnectorViewsContainer
		{
			get { return _PartConnectorViewsContainer; }
		}

		protected Canvas _PartDragAndSelectionCanvas;
		public Canvas PartDragAndSelectionCanvas
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

			ContextMenu = GetTemplateChild( "PART_ContextMenu" ) as ContextMenu;
			ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Right;

			_PartDragAndSelectionCanvas = GetTemplateChild( "PART_DragAndSelectionCanvas" ) as Canvas;
		}

		#endregion // Template

		#region Mouse Events

		private Point _PrevPosition;

		protected override void OnMouseLeftButtonDown( MouseButtonEventArgs e )
		{
			base.OnMouseLeftButtonDown( e );

			if( null == _ViewModel )
			{
				return;
			}

			Keyboard.Focus( this );

			_PrevPosition = e.GetPosition( this );

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

				NodeGraphManager.This.StartDragSelection( _ViewModel.Model, _PrevPosition );
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
				Point position = e.GetPosition( this );
				NodeGraphManager.This.DragNode( new Point( position.X - _PrevPosition.X, position.Y - _PrevPosition.Y ) );
				_PrevPosition = position;
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

			NodeGraphManager.This.EndConnection();
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

			if( MouseButtonState.Pressed != e.LeftButton )
			{
				_ViewModel.BuildContextMenuItems( ContextMenu.Items, Mouse.GetPosition( this ) );
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
	}
}
