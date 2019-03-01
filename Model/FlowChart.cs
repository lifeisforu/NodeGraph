using NodeGraph.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NodeGraph.Model
{
	[FlowChart()]
	public class FlowChart : ModelBase
	{
		#region Properties

		protected FlowChartViewModel _ViewModel;
		public FlowChartViewModel ViewModel
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

		protected ObservableCollection<Node> _Nodes = new ObservableCollection<Node>();
		public ObservableCollection<Node> Nodes
		{
			get { return _Nodes; }
			set
			{
				if( value != _Nodes )
				{
					_Nodes = value;
					RaisePropertyChanged( "Nodes" );
				}
			}
		}

		protected ObservableCollection<Connector> _Connectors = new ObservableCollection<Connector>();
		public ObservableCollection<Connector> Connectors
		{
			get { return _Connectors; }
			set
			{
				if( value != _Connectors )
				{
					_Connectors = value;
					RaisePropertyChanged( "Connectors" );
				}
			}
		}

		public History.NodeGraphHistory History { get; private set; }

		#endregion // Properties

		#region Constructor

		/// <summary>
		/// Never call this constructor directly. Use GraphManager.CreateFlowChart() method.
		/// </summary>
		public FlowChart( Guid guid ) : base( guid )
		{
			History = new History.NodeGraphHistory( this, 100 );
		}

		#endregion // Constructor

		#region Callbacks

		public virtual void OnCreate()
		{
			if( NodeGraphManager.OutputDebugInfo )
				System.Diagnostics.Debug.WriteLine( "FlowChart.OnCreate()" );
		}

		public virtual void OnPreExecute()
		{
			if( NodeGraphManager.OutputDebugInfo )
				System.Diagnostics.Debug.WriteLine( "FlowChart.OnPreExecute()" );
		}

		public virtual void OnExecute()
		{
			if( NodeGraphManager.OutputDebugInfo )
				System.Diagnostics.Debug.WriteLine( "FlowChart.OnExecute()" );
		}

		public virtual void OnPostExecute()
		{
			if( NodeGraphManager.OutputDebugInfo )
				System.Diagnostics.Debug.WriteLine( "FlowChart.OnPostExecute()" );
		}

		public virtual void OnPreDestroy()
		{
			if( NodeGraphManager.OutputDebugInfo )
				System.Diagnostics.Debug.WriteLine( "FlowChart.OnPreDestroy()" );
		}

		public virtual void OnPostDestroy()
		{
			if( NodeGraphManager.OutputDebugInfo )
				System.Diagnostics.Debug.WriteLine( "FlowChart.OnPostDestroy()" );
		}

		public virtual void OnDeserialize()
		{
			foreach( var node in Nodes )
			{
				node.OnDeserialize();
			}

			foreach( var connector in Connectors )
			{
				connector.OnDeserialize();
			}
		}

		#endregion // Callbacks

		#region Overrides IXmlSerializable

		public override void WriteXml( XmlWriter writer )
		{
			base.WriteXml( writer );

			writer.WriteStartElement( "Nodes" );
			foreach( var node in Nodes )
			{
				writer.WriteStartElement( "Node" );
				node.WriteXml( writer );
				writer.WriteEndElement();
			}
			writer.WriteEndElement();

			writer.WriteStartElement( "Connectors" );
			foreach( var connector in Connectors )
			{
				writer.WriteStartElement( "Connector" );
				connector.WriteXml( writer );
				writer.WriteEndElement();
			}
		}

		public override void ReadXml( XmlReader reader )
		{
			base.ReadXml( reader );

			bool isNodesEnd = false;
			bool isConnectorsEnd = false;

			while( reader.Read() )
			{
				if( XmlNodeType.Element == reader.NodeType )
				{
					if( ( "Node" == reader.Name ) ||
						( "Connector" == reader.Name ) )
					{
						string prevReaderName = reader.Name;

						Guid guid = Guid.Parse( reader.GetAttribute( "Guid" ) );
						Type type = Type.GetType( reader.GetAttribute( "Type" ) );
						FlowChart flowChart = NodeGraphManager.FindFlowChart(
							Guid.Parse( reader.GetAttribute( "Owner" ) ) );

						if( "Node" == prevReaderName )
						{
							Type vmType = Type.GetType( reader.GetAttribute( "ViewModelType" ) );
							double x = double.Parse( reader.GetAttribute( "X" ) );
							double y = double.Parse( reader.GetAttribute( "Y" ) );
							int zIndex = int.Parse( reader.GetAttribute( "ZIndex" ) );

							Node node = NodeGraphManager.CreateNode( true, guid, flowChart, type, x, y, zIndex, vmType );
							node.ReadXml( reader );
						}
						else
						{
							Connector connector = NodeGraphManager.CreateConnector( false, guid, flowChart, type );
							connector.ReadXml( reader );
						}
					}
				}

				if( reader.IsEmptyElement || XmlNodeType.EndElement == reader.NodeType )
				{
					if( "Nodes" == reader.Name )
					{
						isNodesEnd = true;
					}
					else if( "Connectors" == reader.Name )
					{
						isConnectorsEnd = true;
					}
				}

				if( isNodesEnd && isConnectorsEnd )
					break;
			}
		}

		#endregion // Overrides IXmlSerializable
	}
}
