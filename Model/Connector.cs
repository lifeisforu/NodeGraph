using NodeGraph.ViewModel;
using System;
using System.Xml;
using System.Xml.Serialization;

namespace NodeGraph.Model
{
	[Connector()]
	public class Connector : ModelBase, IXmlSerializable
	{
		#region Properties

		public FlowChart FlowChart { get; private set; }

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
		public Connector()
		{

		}

		/// <summary>
		/// Never call this constructor directly. Use GraphManager.CreateConnector() method.
		/// </summary>
		public Connector( Guid guid, FlowChart flowChart ) : base( guid )
		{
			FlowChart = flowChart;
		}

		#endregion // Constructor

		#region Methods

		public bool IsConnectedPort( NodePort port )
		{
			return ( StartPort == port ) || ( EndPort == port );
		}

		#endregion // Methods

		#region Callbacks

		public virtual void OnCreate()
		{
			if( NodeGraphManager.OutputDebugInfo )
				System.Diagnostics.Debug.WriteLine( "Connector.OnCreate()" );
		}

		public virtual void OnPreExecute( NodeFlowPort prevPort )
		{
			if( NodeGraphManager.OutputDebugInfo )
				System.Diagnostics.Debug.WriteLine( "Connector.OnPreExecute()" );
		}

		public virtual void OnExecute( NodeFlowPort prevPort )
		{
			if( NodeGraphManager.OutputDebugInfo )
				System.Diagnostics.Debug.WriteLine( "Connector.OnExecute()" );
		}

		public virtual void OnPostExecute( NodeFlowPort prevPort )
		{
			if( NodeGraphManager.OutputDebugInfo )
				System.Diagnostics.Debug.WriteLine( "Connector.OnPostExecute()" );
		}

		public virtual void OnPreDestroy()
		{
			if( NodeGraphManager.OutputDebugInfo )
				System.Diagnostics.Debug.WriteLine( "Connector.OnPreDestroy()" );
		}

		public virtual void OnPostDestroy()
		{
			if( NodeGraphManager.OutputDebugInfo )
				System.Diagnostics.Debug.WriteLine( "Connector.OnPostDestroy()" );
		}

		public virtual void OnConnect( NodePort port )
		{
			if( NodeGraphManager.OutputDebugInfo )
				System.Diagnostics.Debug.WriteLine( "Connector.OnConnect()" );
		}

		public virtual void OnDisconnect( NodePort port )
		{
			if( NodeGraphManager.OutputDebugInfo )
				System.Diagnostics.Debug.WriteLine( "Connector.OnDisconnect()" );
		}

		#endregion // Callbacks

		#region ModelBase

		public override void WriteXml( XmlWriter writer )
		{
			base.WriteXml( writer );

			writer.WriteAttributeString( "FlowChart", FlowChart.Guid.ToString() );
			writer.WriteAttributeString( "StartPort", StartPort.Guid.ToString() );
			writer.WriteAttributeString( "EndPort", EndPort.Guid.ToString() );
		}

		public override void ReadXml( XmlReader reader )
		{
			base.ReadXml( reader );

			string guidString = reader.GetAttribute( "FlowChart" );
			FlowChart = NodeGraphManager.FindFlowChart( Guid.Parse( guidString ) );

			guidString = reader.GetAttribute( "StartPortGuid" );
			StartPort = NodeGraphManager.FindNodePort( Guid.Parse( guidString ) );

			guidString = reader.GetAttribute( "EndPortGuid" );
			EndPort = NodeGraphManager.FindNodePort( Guid.Parse( guidString ) );
		}

		#endregion // ModelBase
	}
}
