using NodeGraph.Model;
using NodeGraph.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace NodeGraph.View
{
	public class NodeView : ContentControl
	{
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

		public NodeViewModel ViewModel { get; private set; }

		public bool HasConnection
		{
			get { return ( bool )GetValue( HasConnectionProperty ); }
			set { SetValue( HasConnectionProperty, value ); }
		}
		public static readonly DependencyProperty HasConnectionProperty =
			DependencyProperty.Register( "HasConnection", typeof( bool ), typeof( NodeView ), new PropertyMetadata( false ) );

		public Thickness SelectionThickness
		{
			get { return ( Thickness )GetValue( SelectionThicknessProperty ); }
			set { SetValue( SelectionThicknessProperty, value ); }
		}
		public static readonly DependencyProperty SelectionThicknessProperty =
			DependencyProperty.Register( "SelectionThickness", typeof( Thickness ), typeof( NodeView ), new PropertyMetadata( new Thickness( 2.0 ) ) );

		public double CornerRadius
		{
			get { return ( double )GetValue( CornerRadiusProperty ); }
			set { SetValue( CornerRadiusProperty, value ); }
		}
		public static readonly DependencyProperty CornerRadiusProperty =
			DependencyProperty.Register( "CornerRadius", typeof( double ), typeof( NodeView ), new PropertyMetadata( 8.0 ) );

		#endregion // Properties

		#region Constructors

		public NodeView()
		{
			DataContextChanged += NodeView_DataContextChanged;
			Loaded += NodeView_Loaded;
			Unloaded += NodeView_Unloaded;
		}

		private void NodeView_Loaded( object sender, RoutedEventArgs e )
		{
			OnCanvasRenderTransformChanged();
		}

		private void NodeView_Unloaded( object sender, RoutedEventArgs e )
		{
		}

		private void NodeView_DataContextChanged( object sender, DependencyPropertyChangedEventArgs e )
		{
			ViewModel = DataContext as NodeViewModel;
			if( null == ViewModel )
				throw new Exception( "ViewModel must be bound as DataContext in NodeView." );
			ViewModel.View = this;
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

			FlowChart flowChart = ViewModel.Model.FlowChart;

			if( NodeGraphManager.IsConnecting )
			{
				bool bConnected;
				flowChart.History.BeginTransaction( "Creating Connection" );
				{
					bConnected = NodeGraphManager.EndConnection();
				}
				flowChart.History.EndTransaction( !bConnected );
			}

			if( NodeGraphManager.IsSelecting )
			{
				bool bChanged = false;
				flowChart.History.BeginTransaction( "Selecting" );
				{
					bChanged = NodeGraphManager.EndDragSelection( false );
				}
				flowChart.History.EndTransaction( !bChanged );
			}

			if( !NodeGraphManager.AreNodesReallyDragged &&
				NodeGraphManager.MouseLeftDownNode == ViewModel.Model )
			{
				flowChart.History.BeginTransaction( "Selection" );
				{
					NodeGraphManager.TrySelection( flowChart, ViewModel.Model,
						Keyboard.IsKeyDown( Key.LeftCtrl ),
						Keyboard.IsKeyDown( Key.LeftShift ),
						Keyboard.IsKeyDown( Key.LeftAlt ) );
				}
				flowChart.History.EndTransaction( false );
			}

			NodeGraphManager.EndDragNode();

			NodeGraphManager.MouseLeftDownNode = null;

			e.Handled = true;
		}

		private Point _DraggingStartPos;
		private Matrix _ZoomAndPanStartMatrix;
		protected override void OnMouseLeftButtonDown( MouseButtonEventArgs e )
		{
			base.OnMouseLeftButtonDown( e );

			FlowChart flowChart = ViewModel.Model.FlowChart;
			FlowChartView flowChartView = flowChart.ViewModel.View;
			Keyboard.Focus( flowChartView );

			NodeGraphManager.EndConnection();
			NodeGraphManager.EndDragNode();
			NodeGraphManager.EndDragSelection( false );

			NodeGraphManager.MouseLeftDownNode = ViewModel.Model;

			NodeGraphManager.BeginDragNode( flowChart );

			Node node = ViewModel.Model;
			_DraggingStartPos = new Point( node.X, node.Y );

			flowChart.History.BeginTransaction( "Moving node" );

			_ZoomAndPanStartMatrix = flowChartView.ZoomAndPan.Matrix;

			e.Handled = true;
		}

		protected override void OnPreviewMouseLeftButtonUp( MouseButtonEventArgs e )
		{
			base.OnPreviewMouseLeftButtonUp( e );

			if( NodeGraphManager.IsNodeDragging )
			{
				FlowChart flowChart = ViewModel.Model.FlowChart;

				Node node = ViewModel.Model;
				Point delta = new Point( node.X - _DraggingStartPos.X, node.Y - _DraggingStartPos.Y );

				if( ( 0 != ( int )delta.X ) &&
					( 0 != ( int ) delta.Y ) )
				{
					ObservableCollection<Guid> selectionList = NodeGraphManager.GetSelectionList( node.FlowChart );
					foreach( var guid in selectionList )
					{
						Node currentNode = NodeGraphManager.FindNode( guid );

						flowChart.History.AddCommand( new History.NodePropertyCommand(
							"Node.X", currentNode.Guid, "X", currentNode.X - delta.X, currentNode.X ) );
						flowChart.History.AddCommand( new History.NodePropertyCommand(
							"Node.Y", currentNode.Guid, "Y", currentNode.Y - delta.Y, currentNode.Y ) );
					}

					flowChart.History.AddCommand( new History.ZoomAndPanCommand(
						"ZoomAndPan", flowChart, _ZoomAndPanStartMatrix, flowChart.ViewModel.View.ZoomAndPan.Matrix ) );

					flowChart.History.EndTransaction( false );
				}
				else
				{
					flowChart.History.EndTransaction( true );
				}
			}
		}

		protected override void OnMouseMove( MouseEventArgs e )
		{
			base.OnMouseMove( e );

			if( NodeGraphManager.IsNodeDragging &&
				( NodeGraphManager.MouseLeftDownNode == ViewModel.Model ) &&
				!IsSelected )
			{
				Node node = ViewModel.Model;
				FlowChart flowChart = node.FlowChart;
				NodeGraphManager.TrySelection( flowChart, node, false, false, false );
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
			HasConnection = ( 0 < ViewModel.InputFlowPortViewModels.Count ) ||
				( 0 < ViewModel.OutputFlowPortViewModels.Count ) ||
				( 0 < ViewModel.InputPropertyPortViewModels.Count ) ||
				( 0 < ViewModel.OutputPropertyPortViewModels.Count );
		}

		#endregion // Connection

		#region RenderTrasnform

		public void OnCanvasRenderTransformChanged()
		{
			Matrix matrix = ( VisualParent as Canvas ).RenderTransform.Value;
			double scale = matrix.M11;

			SelectionThickness = new Thickness( 2.0 / scale );
			CornerRadius = 8.0 / scale;
		}

		#endregion // RenderTransform
	}
}
