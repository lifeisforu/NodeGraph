using NodeGraph.Model;
using NodeGraph.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
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
	[TemplatePart( Name = "PART_Port", Type = typeof( FrameworkElement ) )]
	public class NodePortView : ContentControl
	{
		#region Fields

		private NodePortViewModel _ViewModel;

		#endregion // Fields

		#region Properties

		public FrameworkElement PartPort{ get; private set; }

		public bool IsInput
		{
			get { return ( bool )GetValue( IsInputProperty ); }
			set { SetValue( IsInputProperty, value ); }
		}
		public static readonly DependencyProperty IsInputProperty =
			DependencyProperty.Register( "IsInput", typeof( bool ), typeof( NodePortView ), new PropertyMetadata( false ) );

		public bool IsFilledPort
		{
			get { return ( bool )GetValue( IsFilledPortProperty ); }
			set { SetValue( IsFilledPortProperty, value ); }
		}
		public static readonly DependencyProperty IsFilledPortProperty =
			DependencyProperty.Register( "IsFilledPort", typeof( bool ), typeof( NodePortView ), new PropertyMetadata( false ) );

		#endregion // Properteis

		#region Template

		static NodePortView()
		{
			DefaultStyleKeyProperty.OverrideMetadata( typeof( NodePortView ), new FrameworkPropertyMetadata( typeof( NodePortView ) ) );
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			PartPort = Template.FindName( "PART_Port", this ) as FrameworkElement;
			if( null == PartPort )
				throw new Exception( "PART_Port is not instantiated in NodePortView" );
		}

		#endregion // Template

		#region Constructor

		public NodePortView( bool isInput )
		{
			IsInput = isInput;
			DataContextChanged += NodePortView_DataContextChanged;
		}

		private void NodePortView_DataContextChanged( object sender, DependencyPropertyChangedEventArgs e )
		{
			_ViewModel = DataContext as NodePortViewModel;
			if( null == _ViewModel )
				throw new Exception( "ViewModel must be bound as DataContext in NodePortView." );
			_ViewModel.View = this;
			_ViewModel.Model.ConnectionChanged += Model_ConnectionChanged;
		}

		#endregion // Constructor

		#region DataContext

		#endregion // DataContext

		#region Mouse Events

		protected override void OnMouseLeftButtonDown( MouseButtonEventArgs e )
		{
			base.OnMouseLeftButtonDown( e );

			if( IsMouseOver && !NodeGraphManager.This.IsConnecting )
			{
				IsFilledPort = true;
				NodeGraphManager.This.BeginConnection( _ViewModel.Model );
			}

			e.Handled = true;
		}

		protected override void OnMouseEnter( MouseEventArgs e )
		{
			base.OnMouseEnter( e );

			if( MouseButtonState.Pressed == e.LeftButton )
			{
				if( NodeGraphManager.This.IsConnecting )
				{
					bool connectable = NodeGraphManager.This.CheckIfConnectable( _ViewModel.Model );
					if( connectable )
					{
						NodeGraphManager.This.SetOtherConnectionPort( _ViewModel.Model );
					}
				}
			}
		}

		protected override void OnLostFocus( RoutedEventArgs e )
		{
			base.OnLostFocus( e );

			if( NodeGraphManager.This.IsConnecting )
			{
				NodeGraphManager.This.SetOtherConnectionPort( null );
			}
		}

		protected override void OnMouseLeave( MouseEventArgs e )
		{
			base.OnMouseLeave( e );

			if( NodeGraphManager.This.IsConnecting )
			{
				NodeGraphManager.This.SetOtherConnectionPort( null );
			}
		}

		protected override void OnMouseRightButtonUp( MouseButtonEventArgs e )
		{
			base.OnMouseRightButtonUp( e );

			if( !NodeGraphManager.This.IsConnecting )
			{
				NodeGraphManager.This.Disconnect( _ViewModel.Model );
			}

			e.Handled = true;
		}

		#endregion // Mouse Events

		#region HitTest

		protected override HitTestResult HitTestCore( PointHitTestParameters hitTestParameters )
		{
			if( VisualTreeHelper.GetDescendantBounds( this ).Contains( hitTestParameters.HitPoint ) )
				return new PointHitTestResult( this, hitTestParameters.HitPoint );

			return null;
		}

		#endregion // HitTest

		#region Connections

		private void Model_ConnectionChanged( object sender, EventArgs e )
		{
			IsFilledPort = ( 0 < _ViewModel.Model.Connectors.Count );
		}

		#endregion // Connections
	}
}
