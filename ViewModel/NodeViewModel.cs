using NodeGraph.Model;
using NodeGraph.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace NodeGraph.ViewModel
{
	[NodeViewModel()]
	public class NodeViewModel : ViewModelBase
	{
		#region Fields

		public NodeView View;

		#endregion // Fields

		#region Properties

		private Node _Model;
		public Node Model
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

		public Visibility InputFlowPortsVisibility
		{
			get { return ( 0 < _InputFlowPortViewModels.Count ) ? Visibility.Visible : Visibility.Collapsed; }
		}

		public Visibility OutputFlowPortsVisibility
		{
			get { return ( 0 < _OutputFlowPortViewModels.Count ) ? Visibility.Visible : Visibility.Collapsed; }
		}

		private ObservableCollection<NodeFlowPortViewModel> _InputFlowPortViewModels = new ObservableCollection<NodeFlowPortViewModel>();
		public ObservableCollection<NodeFlowPortViewModel> InputFlowPortViewModels
		{
			get { return _InputFlowPortViewModels; }
			set
			{
				if( value != _InputFlowPortViewModels )
				{
					_InputFlowPortViewModels = value;
					RaisePropertyChanged( "InputFlowPortViewModels" );
				}
			}
		}

		private ObservableCollection<NodeFlowPortViewModel> _OutputFlowPortViewModels = new ObservableCollection<NodeFlowPortViewModel>();
		public ObservableCollection<NodeFlowPortViewModel> OutputFlowPortViewModels
		{
			get { return _OutputFlowPortViewModels; }
			set
			{
				if( value != _OutputFlowPortViewModels )
				{
					_OutputFlowPortViewModels = value;
					RaisePropertyChanged( "OutputFlowPortViewModels" );
				}
			}
		}

		private bool _IsSelected;
		public bool IsSelected
		{
			get { return _IsSelected; }
			set
			{
				if( value != _IsSelected )
				{
					_IsSelected = value;
					View.OnSelectionChanged( _IsSelected );
					RaisePropertyChanged( "IsSelected" );
				}
			}
		}

		#endregion // Node Properties

		#region NodePropertyPorts

		public Visibility InputPropertyPortsVisibility
		{
			get { return ( 0 < _InputPropertyPortViewModels.Count ) ? Visibility.Visible : Visibility.Collapsed; }
		}

		public Visibility OutputPropertyPortsVisibility
		{
			get { return ( 0 < _OutputPropertyPortViewModels.Count ) ? Visibility.Visible : Visibility.Collapsed; }
		}

		private ObservableCollection<NodePropertyPortViewModel> _InputPropertyPortViewModels = new ObservableCollection<NodePropertyPortViewModel>();
		public ObservableCollection<NodePropertyPortViewModel> InputPropertyPortViewModels
		{
			get { return _InputPropertyPortViewModels; }
			set
			{
				if( value != _InputPropertyPortViewModels )
				{
					_InputPropertyPortViewModels = value;
					RaisePropertyChanged( "InputPropertyPortViewModels" );
				}
			}
		}

		private ObservableCollection<NodePropertyPortViewModel> _OutputPropertyPortViewModels = new ObservableCollection<NodePropertyPortViewModel>();
		public ObservableCollection<NodePropertyPortViewModel> OutputPropertyPortViewModels
		{
			get { return _OutputPropertyPortViewModels; }
			set
			{
				if( value != _OutputPropertyPortViewModels )
				{
					_OutputPropertyPortViewModels = value;
					RaisePropertyChanged( "OutputPropertyPortViewModels" );
				}
			}
		}

		#endregion // NodePropertyPorts

		#region Constructors

		public NodeViewModel( Node node )
		{
			Model = node ?? throw new ArgumentException( "Node can not be null in NodeViewModel constructor" );
		}

		#endregion // Constructors
		
		#region Connection Events

		public virtual void OnConnectionRemoved( NodePortViewModel portViewModel )
		{
			if( null != View )
			{
				View.OnPortConnectionChanged();
			}
		}

		public virtual void OnConnectionAdded( NodePortViewModel portViewModel )
		{
			if( null != View )
			{
				View.OnPortConnectionChanged();
			}
		}

		#endregion // Connection Events

		#region ContextMenu

		public delegate void BuildContextMenuEventHandler( object sender, BuildContextMenuEventArgs e );

		public static event BuildContextMenuEventHandler BuildContextMenu;

		public static bool ContextMenuEnabled
		{
			get { return ( null != BuildContextMenu ) && ( 0 < BuildContextMenu.GetInvocationList().Length ); }
		}

		public void InvokeBuildContextMenuEvent( BuildContextMenuEventArgs e )
		{
			BuildContextMenu?.Invoke( this, e );
		}

		#endregion // ContextMenu
	}
}
