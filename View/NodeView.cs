using NodeGraph.Model;
using NodeGraph.ViewModel;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

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

		#region Constructors

		static NodeView()
		{
			DefaultStyleKeyProperty.OverrideMetadata( typeof( NodeView ), new FrameworkPropertyMetadata( typeof( NodeView ) ) );
		}

		public NodeView()
		{
			LayoutUpdated += NodeView_LayoutUpdated;
			DataContextChanged += NodeView_DataContextChanged;
		}

		private void NodeView_DataContextChanged( object sender, DependencyPropertyChangedEventArgs e )
		{
			_ViewModel = DataContext as NodeViewModel;
			if( null == _ViewModel )
				throw new Exception( "ViewModel must be bound as DataContext in NodeView." );
			_ViewModel.View = this;
		}

		private void NodeView_LayoutUpdated( object sender, EventArgs e )
		{
			
		}

		#endregion // Constructors

		#region Template Events

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
		}

		#endregion // Template Events

		#region Mouse Events

		protected override void OnMouseLeftButtonDown( MouseButtonEventArgs e )
		{
			base.OnMouseLeftButtonDown( e );

			if( !NodeGraphManager.This.IsConnecting && 
				!NodeGraphManager.This.IsNodeDragging &&
				!NodeGraphManager.This.IsSelecting )
			{
				NodeGraphManager.This.TrySelection( _ViewModel.Model,
					Keyboard.IsKeyDown( Key.LeftCtrl ), 
					Keyboard.IsKeyDown( Key.LeftShift ),
					Keyboard.IsKeyDown( Key.LeftAlt ) );
				NodeGraphManager.This.StartDragNode( _ViewModel.Model );
			}
		}

		protected override void OnMouseRightButtonUp( MouseButtonEventArgs e )
		{
			base.OnMouseRightButtonUp( e );

			e.Handled = true;
		}

		#endregion // Mouse Events

		#region Selection

		public void OnSelectionChanged( bool isSelected )
		{
			IsSelected = isSelected;
		}

		#endregion // Selection

		#region Connections

		public virtual void OnPortConnectionChanged()
		{

		}

		#endregion // Connections
	}
}
