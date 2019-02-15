using NodeGraph.ViewModel;
using System;

namespace NodeGraph.Model
{
	[Connector()]
	public class Connector : ModelBase
	{
		#region Fields

		public readonly FlowChart Owner;

		#endregion // Fields

		#region Properties

		protected ConnectorViewModel _ViewModel;
		public ConnectorViewModel ViewModel
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

		protected NodePort _StartPort;
		public NodePort StartPort
		{
			get { return _StartPort; }
			set
			{
				if( value != _StartPort )
				{
					_StartPort = value;
					RaisePropertyChanged( "StartPort" );
				}
			}
		}

		protected NodePort _EndPort;
		public NodePort EndPort
		{
			get { return _EndPort; }
			set
			{
				if( value != _EndPort )
				{
					_EndPort = value;
					RaisePropertyChanged( "EndPort" );
				}
			}
		}

		#endregion // Properties

		#region Constructor

		/// <summary>
		/// Never call this constructor directly. Use GraphManager.CreateConnector() method.
		/// </summary>
		public Connector( Guid guid, FlowChart flowChart ) : base( guid )
		{
			Owner = flowChart;
		}

		#endregion // Constructor

		#region Methods

		public bool IsConnectedPort( NodePort port )
		{
			return ( StartPort == port ) || ( EndPort == port );
		}

		#endregion // Methods

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
