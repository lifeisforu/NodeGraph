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
	[ TemplatePart( Name = "PART_Port", Type = typeof( FrameworkElement ) )]
	public class NodePortView : ContentControl
	{
		#region Properties

		public NodePortViewModel ViewModel { get; private set; }

		public FrameworkElement PartPort { get; private set; }

		public TextBlock PartToolTip { get; private set; }

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

		public bool IsPortEnabled
		{
			get { return ( bool )GetValue( IsPortEnabledProperty ); }
			set { SetValue( IsPortEnabledProperty, value ); }
		}
		public static readonly DependencyProperty IsPortEnabledProperty =
			DependencyProperty.Register( "IsPortEnabled", typeof( bool ), typeof( NodePortView ), new PropertyMetadata( true ) );

		public bool ToolTipVisibility
		{
			get { return ( bool )GetValue( ToolTipVisibilityProperty ); }
			set { SetValue( ToolTipVisibilityProperty, value ); }
		}
		public static readonly DependencyProperty ToolTipVisibilityProperty =
			DependencyProperty.Register( "ToolTipVisibility", typeof( bool ), typeof( NodePortView ), new PropertyMetadata( false ) );

		public string ToolTipText
		{
			get { return ( string )GetValue( ToolTipTextProperty ); }
			set { SetValue( ToolTipTextProperty, value ); }
		}
		public static readonly DependencyProperty ToolTipTextProperty =
			DependencyProperty.Register( "ToolTipText", typeof( string ), typeof( NodePortView ), new PropertyMetadata( "" ) );

		public bool IsConnectorMouseOver
		{
			get { return ( bool )GetValue( IsConnectorMouseOverProperty ); }
			set { SetValue( IsConnectorMouseOverProperty, value ); }
		}
		public static readonly DependencyProperty IsConnectorMouseOverProperty =
			DependencyProperty.Register( "IsConnectorMouseOver", typeof( bool ), typeof( NodePortView ), new PropertyMetadata( false ) );

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
			Loaded += NodePortView_Loaded;
		}

		#endregion // Constructor

		#region Events

		private void NodePortView_Loaded( object sender, RoutedEventArgs e )
		{
			SynchronizeProperties();
		}

		private void NodePortView_DataContextChanged( object sender, DependencyPropertyChangedEventArgs e )
		{
			ViewModel = DataContext as NodePortViewModel;
			if( null == ViewModel )
				throw new Exception( "ViewModel must be bound as DataContext in NodePortView." );
			ViewModel.View = this;
			ViewModel.PropertyChanged += ViewModelPropertyChanged;

			SynchronizeProperties();
		}

		protected virtual void SynchronizeProperties()
		{
			if( null == ViewModel )
			{
				return;
			}

			NodePort port = ViewModel.Model;
			IsInput = port.IsInput;
			IsFilledPort = ( 0 < port.Connectors.Count );
			IsPortEnabled = port.IsPortEnabled;
			IsEnabled = port.IsEnabled;
		}

		protected virtual void ViewModelPropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
		{
			SynchronizeProperties();
		}

		#endregion // Events

		#region Mouse Events

		protected override void OnMouseLeftButtonDown( MouseButtonEventArgs e )
		{
			base.OnMouseLeftButtonDown( e );

			Node node = ViewModel.Model.Owner;
			FlowChart flowChart = node.Owner;
			Keyboard.Focus( flowChart.ViewModel.View );

			if( Keyboard.IsKeyDown( Key.LeftCtrl ) )
			{
				NodeGraphManager.DisconnectAll( ViewModel.Model );
			}
			else if( !NodeGraphManager.IsConnecting )
			{
				IsFilledPort = true;
				NodeGraphManager.BeginConnection( ViewModel.Model );
			}

			e.Handled = true;
		}

		protected override void OnMouseEnter( MouseEventArgs e )
		{
			base.OnMouseEnter( e );

			if( MouseButtonState.Pressed == e.LeftButton )
			{
				if( NodeGraphManager.IsConnecting )
				{
					string error;
					bool connectable = NodeGraphManager.CheckIfConnectable( ViewModel.Model, out error );
					if( connectable )
					{
						NodeGraphManager.SetOtherConnectionPort( ViewModel.Model );
						ToolTipVisibility = false;
					}
					else
					{
						if( string.IsNullOrEmpty( error ) )
						{
							ToolTipVisibility = false;
						}
						else
						{
							ToolTipText = error;
							ToolTipVisibility = true;
						}
					}
				}
			}
		}

		protected override void OnMouseLeave( MouseEventArgs e )
		{
			base.OnMouseLeave( e );

			if( NodeGraphManager.IsConnecting )
			{
				NodeGraphManager.SetOtherConnectionPort( null );
			}

			ToolTipVisibility = false;
		}

		protected override void OnLostFocus( RoutedEventArgs e )
		{
			base.OnLostFocus( e );

			if( NodeGraphManager.IsConnecting )
			{
				NodeGraphManager.SetOtherConnectionPort( null );
			}

			ToolTipVisibility = false;
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
	}
}
