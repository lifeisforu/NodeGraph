using NodeGraph.Model;
using NodeGraph.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace NodeGraph.View
{
	[TemplatePart( Name = "PART_ConnectorViewsContainer", Type = typeof( FrameworkElement ) )]
	[TemplatePart( Name = "PART_NodeViewsContainer", Type = typeof( FrameworkElement ) )]
	[TemplatePart( Name = "PART_DragAndSelectionCanvas", Type = typeof( FrameworkElement ) )]
	public class FlowChartView : ContentControl
	{
		#region Fields

		protected DispatcherTimer _Timer = new DispatcherTimer();
		protected double _CurrentTime = 0.0;

		#endregion // Fields

		#region Properties

		public FlowChartViewModel ViewModel { get; private set; }

		private ZoomAndPan _ZoomAndPan = new ZoomAndPan();
		public ZoomAndPan ZoomAndPan
		{
			get { return _ZoomAndPan; }
		}

		protected FrameworkElement _NodeCanvas;
		public FrameworkElement NodeCanvas
		{
			get { return _NodeCanvas; }
		}

		protected FrameworkElement _ConnectorCanvas;
		public FrameworkElement ConnectorCanvas
		{
			get { return _ConnectorCanvas; }
		}

		protected FrameworkElement _PartConnectorViewsContainer;
		public FrameworkElement PartConnectorViewsContainer
		{
			get { return _PartConnectorViewsContainer; }
		}

		protected FrameworkElement _PartNodeViewsContainer;
		public FrameworkElement PartNodeViewsContainer
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

			SizeChanged += FlowChartView_SizeChanged;

			_Timer.Interval = new TimeSpan( 0, 0, 0, 0, 33 );
			_Timer.Tick += Timer_Tick;
			_Timer.Start();
		}

		#endregion // Constructors

		#region Template

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_PartConnectorViewsContainer = GetTemplateChild( "PART_ConnectorViewsContainer" ) as FrameworkElement;
			if( null == _PartConnectorViewsContainer )
				throw new Exception( "PART_ConnectorViewsContainer can not be null in FlowChartView" );

			_PartNodeViewsContainer = GetTemplateChild( "PART_NodeViewsContainer" ) as FrameworkElement;
			if( null == _PartNodeViewsContainer )
				throw new Exception( "PART_NodeViewsContainer can not be null in FlowChartView" );

			_PartDragAndSelectionCanvas = GetTemplateChild( "PART_DragAndSelectionCanvas" ) as FrameworkElement;
			if( null == _PartDragAndSelectionCanvas )
				throw new Exception( "PART_DragAndSelectionCanvas can not be null in FlowChartView" );
		}

		#endregion // Template

		#region Events

		private void FlowChartView_SizeChanged( object sender, SizeChangedEventArgs e )
		{
			_ZoomAndPan.ViewWidth = ActualWidth;
			_ZoomAndPan.ViewHeight = ActualHeight;
		}

		private void FlowChartView_DataContextChanged( object sender, DependencyPropertyChangedEventArgs e )
		{
			ViewModel = DataContext as FlowChartViewModel;
			if( null == ViewModel )
				return;

			ViewModel.View = this;

			if( null == _ConnectorCanvas )
			{
				_ConnectorCanvas = ViewUtil.FindChild<Canvas>( _PartNodeViewsContainer );
				if( null == _PartDragAndSelectionCanvas )
					throw new Exception( "Canvas can not be null in PART_ConnectorViewsContainer" );
			}

			if( null == _NodeCanvas )
			{
				_NodeCanvas = ViewUtil.FindChild<Canvas>( _PartNodeViewsContainer );
				if( null == _PartDragAndSelectionCanvas )
					throw new Exception( "Canvas can not be null in PART_NodeViewsContainer" );
			}

			_ZoomAndPan.UpdateTransform += _ZoomAndPan_UpdateTransform;
		}

		#endregion // Events

		#region Mouse Events

		private void _ZoomAndPan_UpdateTransform()
		{
			_NodeCanvas.RenderTransform = new MatrixTransform( _ZoomAndPan.Matrix );

			foreach( var pair in NodeGraphManager.Nodes )
			{
				NodeView nodeView = pair.Value.ViewModel.View;
				nodeView.OnCanvasRenderTransformChanged();
			}
		}

		protected Point _RightButtonDownPos;
		protected Point _LeftButtonDownPos;
		protected Point _PrevMousePos;
		protected bool _IsDraggingCanvas;

		protected override void OnMouseLeftButtonDown( MouseButtonEventArgs e )
		{
			base.OnMouseLeftButtonDown( e );

			if( null == ViewModel )
			{
				return;
			}

			Keyboard.Focus( this );

			_ZoomAndPanStartMatrix = ZoomAndPan.Matrix;

			_LeftButtonDownPos = e.GetPosition( this );
			_PrevMousePos = _LeftButtonDownPos;

			if( !NodeGraphManager.IsNodeDragging &&
				!NodeGraphManager.IsConnecting &&
				!NodeGraphManager.IsSelecting )
			{
				Point mousePos = e.GetPosition( this );

				NodeGraphManager.BeginDragSelection( ViewModel.Model,
					_ZoomAndPan.MatrixInv.Transform( mousePos ) );

				ViewModel.SelectionStartX = mousePos.X;
				ViewModel.SelectionWidth = 0;
				ViewModel.SelectionStartY = mousePos.Y;
				ViewModel.SelectionHeight = 0;

				bool bCtrl = Keyboard.IsKeyDown( Key.LeftCtrl );
				bool bShift = Keyboard.IsKeyDown( Key.LeftShift );
				bool bAlt = Keyboard.IsKeyDown( Key.LeftAlt );

				if( !bCtrl && !bShift && !bAlt )
				{
					NodeGraphManager.DeselectAllNodes( ViewModel.Model );
				}
			}
		}

		protected override void OnMouseLeftButtonUp( MouseButtonEventArgs e )
		{
			base.OnMouseLeftButtonUp( e );

			if( null == ViewModel )
			{
				return;
			}

			FlowChart flowChart = ViewModel.Model;

			NodeGraphManager.EndConnection();
			NodeGraphManager.EndDragNode();

			if( NodeGraphManager.IsSelecting )
			{
				bool bChanged = false;
				flowChart.History.BeginTransaction( "Selecting" );
				{
					bChanged = NodeGraphManager.EndDragSelection( false );
				}

				Point mousePos = e.GetPosition( this );

				if( ( 0 != ( int )( mousePos.X - _LeftButtonDownPos.X ) ) ||
					( 0 != ( int )( mousePos.Y - _LeftButtonDownPos.Y ) ) )
				{
					flowChart.History.AddCommand( new History.ZoomAndPanCommand(
						"ZoomAndPan", ViewModel.Model, _ZoomAndPanStartMatrix, ZoomAndPan.Matrix ) );
					bChanged = true;
				}

				flowChart.History.EndTransaction( !bChanged );
			}
			
		}

		protected override void OnMouseRightButtonDown( MouseButtonEventArgs e )
		{
			base.OnMouseRightButtonDown( e );

			if( null == ViewModel )
			{
				return;
			}

			Keyboard.Focus( this );

			_RightButtonDownPos = e.GetPosition( this );

			_ZoomAndPanStartMatrix = ZoomAndPan.Matrix;

			if( !NodeGraphManager.IsDragging )
			{
				_IsDraggingCanvas = true;

				Mouse.Capture( this, CaptureMode.SubTree );

				History.NodeGraphHistory history = ViewModel.Model.History;
				history.BeginTransaction( "Panning" );
			}
		}

		protected override void OnMouseRightButtonUp( MouseButtonEventArgs e )
		{
			base.OnMouseRightButtonUp( e );

			if( null == ViewModel )
			{
				return;
			}

			NodeGraphManager.EndConnection();
			NodeGraphManager.EndDragNode();
			NodeGraphManager.EndDragSelection( true );

			Point mousePos = Mouse.GetPosition( this );
			Point diff = new Point(
				Math.Abs( _RightButtonDownPos.X - mousePos.X ),
				Math.Abs( _RightButtonDownPos.Y - mousePos.Y ) );

			bool wasDraggingCanvas = ( 5.0 < diff.X ) || ( 5.0 < diff.Y );

			if( _IsDraggingCanvas )
			{
				_IsDraggingCanvas = false;
				Mouse.Capture( null );

				History.NodeGraphHistory history = ViewModel.Model.History;
				if( wasDraggingCanvas )
				{
					history.AddCommand( new History.ZoomAndPanCommand(
						"ZoomAndPan", ViewModel.Model, _ZoomAndPanStartMatrix, ZoomAndPan.Matrix ) );

					history.EndTransaction( false );
				}
				else
				{
					history.EndTransaction( true );
				}
			}

			if( !wasDraggingCanvas )
			{
				HitTestResult hitResult = VisualTreeHelper.HitTest( this, mousePos );
				if( ( null != hitResult ) && ( null != hitResult.VisualHit ) )
				{
					object sender = null;

					BuildContextMenuArgs args = new BuildContextMenuArgs();
					args.ViewSpaceMouseLocation = mousePos;
					args.ModelSpaceMouseLocation = _ZoomAndPan.MatrixInv.Transform( mousePos );

					DependencyObject hit = hitResult.VisualHit;
					NodePortView portView = ViewUtil.FindFirstParent< NodePortView >( hit );
					if( null != portView )
					{
						sender = portView;
						if( typeof( NodePropertyPort ).IsAssignableFrom( portView.ViewModel.Model.GetType() ) )
						{
							args.ModelType = ModelType.PropertyPort;
						}
						else if( typeof( NodeFlowPort ).IsAssignableFrom( portView.ViewModel.Model.GetType() ) )
						{
							args.ModelType = ModelType.FlowPort;
						}
					}
					else
					{
						NodeView nodeView = ViewUtil.FindFirstParent< NodeView >( hit );
						if( null != nodeView )
						{
							sender = nodeView;
							args.ModelType = ModelType.Node;
						}
						else
						{
							sender = this;
							args.ModelType = ModelType.FlowChart;
						}
					}

					if( null != sender )
					{
						ContextMenu = new ContextMenu();
						ContextMenu.Closed += ContextMenu_Closed;
						args.ContextMenu = ContextMenu;

						if( !NodeGraphManager.InvokeBuildContextMenu( sender, args ) )
						{
							ContextMenu = null;
						}
					}
				}
			}
		}

		private void ContextMenu_Closed( object sender, RoutedEventArgs e )
		{
			ContextMenu = null;
		}

		private void UpdateDragging( Point mousePos, Point delta )
		{
			if( NodeGraphManager.IsConnecting )
			{
				NodeGraphManager.UpdateConnection( mousePos );
			}
			else if( NodeGraphManager.IsNodeDragging )
			{
				double invScale = 1.0f / _ZoomAndPan.Scale;
				NodeGraphManager.DragNode( new Point( delta.X * invScale, delta.Y * invScale ) );
			}
			else if( NodeGraphManager.IsSelecting )
			{
				// gather nodes in area.

				bool bCtrl = Keyboard.IsKeyDown( Key.LeftCtrl );
				bool bShift = Keyboard.IsKeyDown( Key.LeftShift );
				bool bAlt = Keyboard.IsKeyDown( Key.LeftAlt );

				NodeGraphManager.UpdateDragSelection( ViewModel.Model,
					_ZoomAndPan.MatrixInv.Transform( mousePos ), bCtrl, bShift, bAlt );

				Point startPos = _ZoomAndPan.Matrix.Transform( NodeGraphManager.SelectingStartPoint );

				Point selectionStart = new Point( Math.Min( startPos.X, mousePos.X ), Math.Min( startPos.Y, mousePos.Y ) );
				Point selectionEnd = new Point( Math.Max( startPos.X, mousePos.X ), Math.Max( startPos.Y, mousePos.Y ) );

				ViewModel.SelectionStartX = selectionStart.X;
				ViewModel.SelectionStartY = selectionStart.Y;
				ViewModel.SelectionWidth = selectionEnd.X - selectionStart.X;
				ViewModel.SelectionHeight = selectionEnd.Y - selectionStart.Y;
			}
		}

		protected override void OnMouseMove( MouseEventArgs e )
		{
			base.OnMouseMove( e );

			if( null == ViewModel )
			{
				return;
			}

			Point mousePos = e.GetPosition( this );

			MouseArea area = CheckMouseArea();
			Point delta = new Point( mousePos.X - _PrevMousePos.X,
				mousePos.Y - _PrevMousePos.Y );

			if( NodeGraphManager.IsDragging )
			{
				UpdateDragging( mousePos, delta );
			}
			else
			{
				if( _IsDraggingCanvas )
				{
					_ZoomAndPan.StartX -= delta.X;
					_ZoomAndPan.StartY -= delta.Y;
				}
			}

			_PrevMousePos = mousePos;
		}

		protected override void OnMouseLeave( MouseEventArgs e )
		{
			base.OnMouseLeave( e );

			if( null == ViewModel )
			{
				return;
			}

			NodeGraphManager.EndConnection();
			NodeGraphManager.EndDragNode();
			NodeGraphManager.EndDragSelection( true );
		}

		protected override void OnLostFocus( RoutedEventArgs e )
		{
			base.OnLostFocus( e );

			if( null == ViewModel )
			{
				return;
			}

			NodeGraphManager.EndConnection();
			NodeGraphManager.EndDragNode();
			NodeGraphManager.EndDragSelection( true );

			if( _IsDraggingCanvas )
			{
				_IsDraggingCanvas = false;
				Mouse.Capture( null );

				History.NodeGraphHistory history = ViewModel.Model.History;
				history.EndTransaction( true );
			}
		}

		private bool _IsWheeling = false;
		private double _WheelStartTime = 0.0;
		private Matrix _ZoomAndPanStartMatrix;
		protected override void OnMouseWheel( MouseWheelEventArgs e )
		{
			if( !_IsWheeling )
			{
				History.NodeGraphHistory history = ViewModel.Model.History;
				history.BeginTransaction( "Zooming" );
				_ZoomAndPanStartMatrix = ZoomAndPan.Matrix;
			}

			_WheelStartTime = _CurrentTime;
			_IsWheeling = true;

			base.OnMouseWheel( e );

			double newScale = _ZoomAndPan.Scale;
			newScale += ( 0.0 > e.Delta ) ? -0.05 : 0.05;
			newScale = Math.Max( 0.1, Math.Min( 1.0, newScale ) );

			Point vsZoomCenter = e.GetPosition( this );
			Point zoomCenter = _ZoomAndPan.MatrixInv.Transform( vsZoomCenter );

			_ZoomAndPan.Scale = newScale;

			Point vsNextZoomCenter = _ZoomAndPan.Matrix.Transform( zoomCenter );
			Point vsDelta = new Point( vsZoomCenter.X - vsNextZoomCenter.X, vsZoomCenter.Y - vsNextZoomCenter.Y );

			_ZoomAndPan.StartX -= vsDelta.X;
			_ZoomAndPan.StartY -= vsDelta.Y;
		}

		#endregion // Mouse Events

		#region Timer Events

		private void Timer_Tick( object sender, EventArgs e )
		{
			_CurrentTime += _Timer.Interval.Milliseconds;

			if( NodeGraphManager.IsDragging )
			{
				MouseArea area = CheckMouseArea();

				if( MouseArea.None != area )
				{
					Point delta = new Point( 0.0, 0.0 );
					if( MouseArea.Left == ( area & MouseArea.Left ) )
						delta.X = -10.0;
					if( MouseArea.Right == ( area & MouseArea.Right ) )
						delta.X = 10.0;
					if( MouseArea.Top == ( area & MouseArea.Top ) )
						delta.Y = -10.0;
					if( MouseArea.Bottom == ( area & MouseArea.Bottom ) )
						delta.Y = 10.0;

					Point mousePos = Mouse.GetPosition( this );
					UpdateDragging(
						new Point( mousePos.X + delta.X, mousePos.Y + delta.Y ), // virtual mouse-position.
						delta ); // virtual delta.

					_ZoomAndPan.StartX += delta.X;
					_ZoomAndPan.StartY += delta.Y;
				}
			}
			else if( _IsWheeling )
			{
				if( 200 < ( _CurrentTime - _WheelStartTime ) )
				{
					_IsWheeling = false;

					History.NodeGraphHistory history = ViewModel.Model.History;

					history.AddCommand( new History.ZoomAndPanCommand(
						"ZoomAndPan", ViewModel.Model, _ZoomAndPanStartMatrix, ZoomAndPan.Matrix ) );

					history.EndTransaction( false );
				}
			}
			else
			{
				_CurrentTime = 0.0;
			}
		}

		#endregion // Timer Events

		#region Keyboard Events

		protected override void OnKeyDown( KeyEventArgs e )
		{
			base.OnKeyDown( e );

			if( null == ViewModel )
			{
				return;
			}

			if( Key.Delete == e.Key )
			{
				FlowChart flowChart = ViewModel.Model;
				flowChart.History.BeginTransaction( "Destroy Selected Nodes" );
				{
					NodeGraphManager.DestroySelectedNodes( ViewModel.Model );
				}
				flowChart.History.EndTransaction( false );
			}
			else if( Key.Escape == e.Key )
			{
				FlowChart flowChart = ViewModel.Model;
				flowChart.History.BeginTransaction( "Destroy Selected Nodes" );
				{
					NodeGraphManager.DeselectAllNodes( ViewModel.Model );
				}
				flowChart.History.EndTransaction( false );
			}
			else if( Key.A == e.Key )
			{
				if( Keyboard.IsKeyDown( Key.LeftCtrl ) )
				{
					NodeGraphManager.SelectAllNodes( ViewModel.Model );
				}
				else
				{
					FitNodesToView( false );
				}
			}
			else if( Key.F == e.Key )
			{
				FitNodesToView( true );
			}
			else if( Key.Z == e.Key )
			{
				if( Keyboard.IsKeyDown( Key.LeftCtrl ) )
				{
					History.NodeGraphHistory history = ViewModel.Model.History;
					history.Undo();
				}
			}
			else if( Key.Y == e.Key )
			{
				if( Keyboard.IsKeyDown( Key.LeftCtrl ) )
				{
					History.NodeGraphHistory history = ViewModel.Model.History;
					history.Redo();
				}
			}
		}

		#endregion // Keyboard Events

		#region Area

		public enum MouseArea : uint
		{
			None = 0x00000000,
			Left = 0x00000001,
			Right = 0x00000002,
			Top = 0x00000004,
			Bottom = 0x00000008,
		}

		public MouseArea CheckMouseArea()
		{
			Point absPosition = Mouse.GetPosition( this );
			Point absTopLeft = new Point( 0.0, 0.0 );
			Point absBottomRight = new Point( ActualWidth, ActualHeight );

			MouseArea area = MouseArea.None;

			if( absPosition.X < ( absTopLeft.X + 2.0 ) )
				area |= MouseArea.Left;
			if( absPosition.X > ( absBottomRight.X - 2.0 ) )
				area |= MouseArea.Right;
			if( absPosition.Y < ( absTopLeft.Y + 2.0 ) )
				area |= MouseArea.Top;
			if( absPosition.Y > ( absBottomRight.Y - 2.0 ) )
				area |= MouseArea.Bottom;

			return area;
		}

		#endregion // Area

		#region Fitting.

		public void FitNodesToView( bool bOnlySelected )
		{
			double minX;
			double maxX;
			double minY;
			double maxY;
			NodeGraphManager.CalculateContentSize( ViewModel.Model, bOnlySelected, out minX, out maxX, out minY, out maxY );
			if( ( minX == maxX ) || ( minY == maxY ) )
			{
				return;
			}

			FlowChart flowChart = ViewModel.Model;
			flowChart.History.BeginTransaction( "Destroy Selected Nodes" );
			{
				_ZoomAndPanStartMatrix = ZoomAndPan.Matrix;
				
				double vsWidth = _ZoomAndPan.ViewWidth;
				double vsHeight = _ZoomAndPan.ViewHeight;

				Point margin = new Point( vsWidth * 0.05, vsHeight * 0.05 );
				minX -= margin.X;
				minY -= margin.Y;
				maxX += margin.X;
				maxY += margin.Y;

				double contentWidth = maxX - minX;
				double contentHeight = maxY - minY;

				_ZoomAndPan.StartX = ( minX + maxX - vsWidth ) * 0.5;
				_ZoomAndPan.StartY = ( minY + maxY - vsHeight ) * 0.5;
				_ZoomAndPan.Scale = 1.0;

				Point vsZoomCenter = new Point( vsWidth * 0.5, vsHeight * 0.5 );
				Point zoomCenter = _ZoomAndPan.MatrixInv.Transform( vsZoomCenter );

				double newScale = Math.Min( vsWidth / contentWidth, vsHeight / contentHeight );
				_ZoomAndPan.Scale = Math.Max( 0.1, Math.Min( 1.0, newScale ) );

				Point vsNextZoomCenter = _ZoomAndPan.Matrix.Transform( zoomCenter );
				Point vsDelta = new Point( vsZoomCenter.X - vsNextZoomCenter.X, vsZoomCenter.Y - vsNextZoomCenter.Y );

				_ZoomAndPan.StartX -= vsDelta.X;
				_ZoomAndPan.StartY -= vsDelta.Y;

				if( 0 != ( int )( _ZoomAndPan.Matrix.OffsetX - _ZoomAndPanStartMatrix.OffsetX ) ||
					0 != ( int )( _ZoomAndPan.Matrix.OffsetX - _ZoomAndPanStartMatrix.OffsetX ) )
				{
					flowChart.History.AddCommand( new History.ZoomAndPanCommand(
						"ZoomAndPan", ViewModel.Model, _ZoomAndPanStartMatrix, ZoomAndPan.Matrix ) );
				}
			}
			flowChart.History.EndTransaction( false );
		}

		#endregion // Fitting.
	}

	public class ZoomAndPan
	{
		#region Properties

		private double _ViewWidth;
		public double ViewWidth
		{
			get { return _ViewWidth; }
			set
			{
				if( value != _ViewWidth )
				{
					_ViewWidth = value;
				}
			}
		}

		private double _ViewHeight;
		public double ViewHeight
		{
			get { return _ViewHeight; }
			set
			{
				if( value != _ViewHeight )
				{
					_ViewHeight = value;
				}
			}
		}

		private double _StartX = 0.0;
		public double StartX
		{
			get { return _StartX; }
			set
			{
				if( value != _StartX )
				{
					_StartX = value;
					_UpdateTransform();
				}
			}
		}

		private double _StartY = 0.0;
		public double StartY
		{
			get { return _StartY; }
			set
			{
				if( value != _StartY )
				{
					_StartY = value;
					_UpdateTransform();
				}
			}
		}

		private double _Scale = 1.0;
		public double Scale
		{
			get { return _Scale; }
			set
			{
				if( value != _Scale )
				{
					_Scale = value;
					_UpdateTransform();
				}
			}
		}

		private Matrix _Matrix = Matrix.Identity;
		public Matrix Matrix
		{
			get { return _Matrix; }
			set
			{
				if( value != _Matrix )
				{
					_Matrix = value;
					_MatrixInv = value;
					_MatrixInv.Invert();
				}
			}
		}

		private Matrix _MatrixInv = Matrix.Identity;
		public Matrix MatrixInv
		{
			get { return _MatrixInv; }
		}


		#endregion // Properties

		#region Methdos

		private void _UpdateTransform()
		{
			Matrix newMatrix = Matrix.Identity;
			newMatrix.Scale( _Scale, _Scale );
			newMatrix.Translate( -_StartX, -_StartY );

			Matrix = newMatrix;

			UpdateTransform?.Invoke();
		}

		#endregion // Methods

		#region Events

		public delegate void UpdateTransformDelegate();
		public event UpdateTransformDelegate UpdateTransform;

		#endregion // Events
	}
}
