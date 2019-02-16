using NodeGraph.Model;
using NodeGraph.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NodeGraph.View
{
	public class ConnectorView : ContentControl
	{
		#region Fields

		protected ConnectorViewModel _ViewModel;

		#endregion // Fields

		#region Properties

		public string CurveData
		{
			get { return ( string )GetValue( CurveDataProperty ); }
			set { SetValue( CurveDataProperty, value ); }
		}
		public static readonly DependencyProperty CurveDataProperty =
			DependencyProperty.Register( "CurveData", typeof( string ), typeof( ConnectorView ), new PropertyMetadata( "" ) );

		#endregion // Properties

		#region Template

		static ConnectorView()
		{
			DefaultStyleKeyProperty.OverrideMetadata( typeof( ConnectorView ), new FrameworkPropertyMetadata( typeof( ConnectorView ) ) );
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
		}

		#endregion // Template

		#region Constructor

		public ConnectorView()
		{
			LayoutUpdated += ConnectorView_LayoutUpdated;
			DataContextChanged += ConnectorView_DataContextChanged;

			ContextMenu = new ContextMenu();
			ContextMenuOpening += ConnectorView_ContextMenuOpening;
		}

		private void ConnectorView_DataContextChanged( object sender, DependencyPropertyChangedEventArgs e )
		{
			_ViewModel = DataContext as ConnectorViewModel;
			if( null == _ViewModel )
				throw new Exception( "ViewModel must be bound as DataContext in ConnectorView." );
			_ViewModel.View = this;
		}

		private void ConnectorView_LayoutUpdated( object sender, EventArgs e )
		{
			BuildCurveData();
		}

		#endregion // Constructor

		#region Curve

		public void BuildCurveData()
		{
			Connector connector = _ViewModel.Model;
			FlowChart flowChart = connector.Owner;
			FlowChartView flowChartView = flowChart.ViewModel.View;

			NodePort startPort = connector.StartPort;
			NodePort endPort = connector.EndPort;

			Point mouseLocation = Mouse.GetPosition( flowChartView );

			Point start = ( null != startPort ) ? ViewUtil.GetRelativeCenterLocation( startPort.ViewModel.View.PartPort, flowChartView ) : mouseLocation;
			Point end = ( null != endPort ) ? ViewUtil.GetRelativeCenterLocation( endPort.ViewModel.View.PartPort, flowChartView ) : mouseLocation;
			Point center = new Point( ( start.X + end.X ) * 0.5, ( start.Y + end.Y ) * 0.5 );

			if( start.X > end.X )
			{
				Point temp = start;
				start = end;
				end = temp;
			}

			double ratio = Math.Min( 1.0, ( center.X - start.X ) / 100.0 );
			Point c0 = start;
			Point c1 = end;
			c0.X += 100 * ratio;
			c1.X -= 100 * ratio;

			CurveData = string.Format( "M{0},{1} C{0},{1} {2},{3} {4},{5} " +
				"M{4},{5} C{4},{5} {6},{7} {8},{9}",
				( int )start.X, ( int )start.Y, // 0, 1
				( int )c0.X, ( int )c0.Y, // 2, 3
				( int )center.X, ( int )center.Y, // 4, 5
				( int )c1.X, ( int )c1.Y, // 6, 7
				( int )end.X, ( int )end.Y ); // 8.9
		}

		#endregion // Curve

		#region Context Menu

		private void ConnectorView_ContextMenuOpening( object sender, ContextMenuEventArgs e )
		{
			if( null == _ViewModel )
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

		#endregion //Context Menu
	}
}
