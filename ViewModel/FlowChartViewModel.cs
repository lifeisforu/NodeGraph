using NodeGraph.Model;
using NodeGraph.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NodeGraph.ViewModel
{
	public class FlowChartViewModel : ViewModelBase
	{
		#region Fields

		public FlowChartView View;

		#endregion // Fields

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

		public FlowChartViewModel( FlowChart flowChart ) : base( flowChart )
		{
			Model = flowChart;
		}

		#endregion // Constructor

		#region Events

		protected override void ModelPropertyChanged( object sender, PropertyChangedEventArgs e )
		{
			base.ModelPropertyChanged( sender, e );

			RaisePropertyChanged( e.PropertyName );
		}

		#endregion // Events
	}
}
