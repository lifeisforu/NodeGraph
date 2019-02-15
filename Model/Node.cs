using NodeGraph.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows.Media;

namespace NodeGraph.Model
{
	public class Node : ModelBase
	{
		#region Fields

		public readonly FlowChart Owner;

		#endregion // Fields

		#region Properties

		protected NodeViewModel _ViewModel;
		public NodeViewModel ViewModel
		{
			get { return _ViewModel; }
			set
			{
				if( value != _ViewModel )
				{
					_ViewModel = value;
					RaisePropertyChanged( "ViewModel" );
				}
			}
		}

		protected string _Header;
		public string Header
		{
			get { return _Header; }
			set
			{
				if( value != _Header )
				{
					_Header = value;
					RaisePropertyChanged( "Header" );
				}
			}
		}

		protected SolidColorBrush _HeaderBackgroundColor;
		public SolidColorBrush HeaderBackgroundColor
		{
			get { return _HeaderBackgroundColor; }
			set
			{
				if( value != _HeaderBackgroundColor )
				{
					_HeaderBackgroundColor = value;
					RaisePropertyChanged( "HeaderBackgroundColor" );
				}
			}
		}

		protected SolidColorBrush _HeaderFontColor;
		public SolidColorBrush HeaderFontColor
		{
			get { return _HeaderFontColor; }
			set
			{
				if( value != _HeaderFontColor )
				{
					_HeaderFontColor = value;
					RaisePropertyChanged( "HeaderFontColor" );
				}
			}
		}

		protected double _X;
		public double X
		{
			get { return _X; }
			set
			{
				if( value != _X )
				{
					_X = value;
					RaisePropertyChanged( "X" );
				}
			}
		}

		protected double _Y;
		public double Y
		{
			get { return _Y; }
			set
			{
				if( value != _Y )
				{
					_Y = value;
					RaisePropertyChanged( "Y" );
				}
			}
		}

		protected int _ZIndex;
		public int ZIndex
		{
			get { return _ZIndex; }
			set
			{
				if( value != _ZIndex )
				{
					_ZIndex = value;
					RaisePropertyChanged( "ZIndex" );
				}
			}
		}

		protected ObservableCollection<NodeFlowPort> _InputFlowPorts = new ObservableCollection<NodeFlowPort>();
		public ObservableCollection<NodeFlowPort> InputFlowPorts
		{
			get { return _InputFlowPorts; }
			set
			{
				if( value != _InputFlowPorts )
				{
					_InputFlowPorts = value;
					RaisePropertyChanged( "InputFlowPorts" );
				}
			}
		}

		protected ObservableCollection<NodeFlowPort> _OutputFlowPorts = new ObservableCollection<NodeFlowPort>();
		public ObservableCollection<NodeFlowPort> OutputFlowPorts
		{
			get { return _OutputFlowPorts; }
			set
			{
				if( value != _OutputFlowPorts )
				{
					_OutputFlowPorts = value;
					RaisePropertyChanged( "OutputFlowPorts" );
				}
			}
		}

		protected ObservableCollection<NodePropertyPort> _InputPropertyPorts = new ObservableCollection<NodePropertyPort>();
		public ObservableCollection<NodePropertyPort> InputPropertyPorts
		{
			get { return _InputPropertyPorts; }
			set
			{
				if( value != _InputPropertyPorts )
				{
					_InputPropertyPorts = value;
					RaisePropertyChanged( "InputPropertyPorts" );
				}
			}
		}

		protected ObservableCollection<NodePropertyPort> _OutputPropertyPorts = new ObservableCollection<NodePropertyPort>();
		public ObservableCollection<NodePropertyPort> OutputPropertyPorts
		{
			get { return _OutputPropertyPorts; }
			set
			{
				if( value != _OutputPropertyPorts )
				{
					_OutputPropertyPorts = value;
					RaisePropertyChanged( "OutputPropertyPorts" );
				}
			}
		}

		public bool AllowCircularConnection { get; private set; }

		#endregion // Properties

		#region Constructor

		/// <summary>
		/// Never call this constructor directly. Use GraphManager.CreateNode() method.
		/// </summary>
		public Node( Guid guid, FlowChart flowChart, bool allowCircularConnection ) : base( guid )
		{
			Owner = flowChart;

			AllowCircularConnection = allowCircularConnection;
		}

		#endregion // Constructor

		#region Destructor

		~Node()
		{
			
		}

		#endregion // Destructor

		#region Create Events

		public event EventHandler Create;

		public void InvokeCreateEvent()
		{
			EventArgs args = new EventArgs();

			OnCreate();

			Create?.Invoke( this, new EventArgs() );
		}

		protected virtual void OnCreate()
		{

		}

		#endregion // Create Events
	}
}
