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

		public Guid OwnerGuid { get; private set; }

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

		protected Guid _StartPortGuid;
		public Guid StartPortGuid
		{
			get { return _StartPortGuid; }
			set
			{
				if( value != _StartPortGuid )
				{
					_StartPortGuid = value;
					RaisePropertyChanged( "StartPortGuid" );
				}
			}
		}

		protected Guid _EndPortGuid;
		public Guid EndPortGuid
		{
			get { return _EndPortGuid; }
			set
			{
				if( value != _EndPortGuid )
				{
					_EndPortGuid = value;
					RaisePropertyChanged( "EndPortGuid" );
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
		public Connector( Guid guid, Guid flowChartGuid ) : base( guid )
		{
			OwnerGuid = flowChartGuid;
		}

		#endregion // Constructor

		#region Methods

		public bool IsConnectedPort( NodePort port )
		{
			return ( StartPortGuid == port.Guid ) || ( EndPortGuid == port.Guid );
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

			writer.WriteAttributeString( "OwnerGuid", OwnerGuid.ToString() );
			writer.WriteAttributeString( "StartPortGuid", StartPortGuid.ToString() );
			writer.WriteAttributeString( "EndPortGuid", StartPortGuid.ToString() );
		}

		public override void ReadXml( XmlReader reader )
		{
			base.ReadXml( reader );

			string guidString = reader.GetAttribute( "OwnerGuid" );
			if( string.IsNullOrEmpty( guidString ) )
				throw new Exception( "OwnerGuid attribute must exist." );
			OwnerGuid = Guid.Parse( guidString );

			guidString = reader.GetAttribute( "StartPortGuid" );
			if( string.IsNullOrEmpty( guidString ) )
				throw new Exception( "StartPortGuid attribute must exist." );
			StartPortGuid = Guid.Parse( guidString );

			guidString = reader.GetAttribute( "EndPortGuid" );
			if( string.IsNullOrEmpty( guidString ) )
				throw new Exception( "EndPortGuid attribute must exist." );
			EndPortGuid = Guid.Parse( guidString );
		}

		#endregion // ModelBase
	}
}
