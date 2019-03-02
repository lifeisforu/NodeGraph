using NodeGraph.Model;
using NodeGraph.View;
using NodeGraph.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Xml;

namespace NodeGraph
{
	public enum SelectionMode
	{
		Overlap,
		Include,
	}

	public class NodeGraphManager
	{
		#region Fields

		public static readonly Dictionary<Guid, FlowChart> FlowCharts = new Dictionary<Guid, FlowChart>();
		public static readonly Dictionary<Guid, Node> Nodes = new Dictionary<Guid, Node>();
		public static readonly Dictionary<Guid, Connector> Connectors = new Dictionary<Guid, Connector>();
		public static readonly Dictionary<Guid, NodeFlowPort> NodeFlowPorts = new Dictionary<Guid, NodeFlowPort>();
		public static readonly Dictionary<Guid, NodePropertyPort> NodePropertyPorts = new Dictionary<Guid, NodePropertyPort>();
		public static readonly Dictionary<Guid, ObservableCollection<Guid>> SelectedNodes = new Dictionary<Guid, ObservableCollection<Guid>>();
		public static bool OutputDebugInfo = false;
		public static SelectionMode SelectionMode = SelectionMode.Overlap;

		#endregion // Fields

		#region FlowChart

		/// <summary>
		/// Create FlowChart with FlowChartViewModel.
		/// </summary>
		/// <param name="isDeserializing">Is in deserializing routine? 
		/// If it is true, OnCreate() callback will not be called, otherwise OnDeserialize will be called.</param>
		/// <param name="guid">Guid of this FlowChart.</param>
		/// <param name="flowChartModelType">Type of FlowChart to be created.</param>
		/// <returns>Created FlowChart instance</returns>
		public static FlowChart CreateFlowChart( bool isDeserializing, Guid guid, Type flowChartModelType )
		{
			//------ create FlowChart.

			var flowChartAttrs = flowChartModelType.GetCustomAttributes( typeof( FlowChartAttribute ), false ) as FlowChartAttribute[];
			if( 1 != flowChartAttrs.Length )
				throw new ArgumentException( string.Format( "{0} must have ONE FlowChartAttribute", flowChartModelType.Name ) );
			var flowChartAttr = flowChartAttrs[ 0 ];

			FlowChart flowChart = Activator.CreateInstance( flowChartModelType, new object[] { guid } ) as FlowChart;
			FlowCharts.Add( flowChart.Guid, flowChart );

			//----- create viewmodel

			flowChart.ViewModel = Activator.CreateInstance( flowChartAttr.ViewModelType, new object[] { flowChart } ) as FlowChartViewModel;

			//----- create selection list.

			ObservableCollection<Guid> selectionList = new ObservableCollection<Guid>();
			selectionList.CollectionChanged += Node_SelectionList_CollectionChanged;
			SelectedNodes.Add( flowChart.Guid, selectionList );

			//----- invocke create callback.

			if( !isDeserializing )
			{
				flowChart.OnCreate();
			}

			//----- return.

			return flowChart;
		}

		public static void DestroyFlowChart( Guid guid )
		{
			FlowChart flowChart;
			if( !FlowCharts.TryGetValue( guid, out flowChart ) )
			{
				return;
			}

			flowChart.OnPreDestroy();

			ObservableCollection<Guid> guids = new ObservableCollection<Guid>();
			foreach( var node in flowChart.Nodes )
			{
				guids.Add( node.Guid );
			}

			foreach( var nodeGuid in guids )
			{
				DestroyNode( nodeGuid );
			}

			if( 0 < flowChart.Connectors.Count )
			{
				throw new InvalidOperationException( "Connectors are not removed." );
			}

			flowChart.OnPostDestroy();

			SelectedNodes.Remove( guid );
			FlowCharts.Remove( guid );
		}

		public static FlowChart FindFlowChart( Guid guid )
		{
			FlowChart flowChart;
			FlowCharts.TryGetValue( guid, out flowChart );
			return flowChart;
		}

		#endregion // FlowChart

		#region Node

		/// <summary>
		/// Create Node with NodeViewModel.
		/// </summary>
		/// <param name="isDeserializing">Is in deserializing routine? 
		/// If it is true, OnCreate() callback will not be called, otherwise OnDeserialize will be called.
		/// If it is true, Node's attribute will not be evaluated. That means flows and properties will not be created automatically by attributes.
		/// All flows and properties will be created during deserialization process.</param>
		/// <param name="guid">Guid for this Node.</param>
		/// <param name="flowChart">Owner FlowChart.</param>
		/// <param name="nodeType">Type of this node.</param>
		/// <param name="x">Location along X axis( Canvas.Left ).</param>
		/// <param name="y">Location along Y axis( Canvas.Top )</param>
		/// <param name="ZIndex">Z index( Canvas.ZIndex ).</param>
		/// <param name="headerOverride">User defined header.</param>
		/// <param name="nodeViewModelTypeOverride">NodeViewModel to override.</param>
		/// <param name="flowPortViewModelTypeOverride">FlowPortViewModel to override.</param>
		/// <param name="propertyPortViewModelTypeOverride">PropertyPortViewmodel to override.</param>
		/// <returns>Created node instance.</returns>
		public static Node CreateNode( bool isDeserializing, Guid guid, FlowChart flowChart, Type nodeType, double x, double y, int ZIndex,
			Type nodeViewModelTypeOverride = null, Type flowPortViewModelTypeOverride = null, Type propertyPortViewModelTypeOverride = null )
		{
			//----- exceptions.

			if( null == flowChart )
				throw new ArgumentNullException( "flowChart of CreateNode() can not be null" );

			if( null == nodeType )
				throw new ArgumentNullException( "nodeType of CreateNode() can not be null" );

			//----- create node from NodeAttribute.

			var nodeAttrs = nodeType.GetCustomAttributes( typeof( NodeAttribute ), false ) as NodeAttribute[];
			if( 1 != nodeAttrs.Length )
				throw new ArgumentException( string.Format( "{0} must have ONE NodeAttribute", nodeType.Name ) );
			var nodeAttr = nodeAttrs[ 0 ];

			// create node model.
			Node node = Activator.CreateInstance( nodeType, new object[] { guid, flowChart } ) as Node;
			node.X = x;
			node.Y = y;
			node.ZIndex = ZIndex;
			Nodes.Add( guid, node );
			// create node viewmodel.
			node.ViewModel = Activator.CreateInstance(
				( null != nodeViewModelTypeOverride ) ? nodeViewModelTypeOverride : nodeAttr.ViewModelType,
				new object[] { node } ) as NodeViewModel;
			flowChart.ViewModel.NodeViewModels.Add( node.ViewModel );
			flowChart.Nodes.Add( node );

			//---- history.

			flowChart.History.AddCommand( new NodeGraph.History.CreateNodeCommand(
				"Creating node", node.Guid, NodeGraphManager.SerializeNode( node ) ) );

			//----- create ports.

			if( !isDeserializing )
			{
				//----- create flowPorts from NodeFlowPortAttribute.

				var flowPortAttrs = nodeType.GetCustomAttributes( typeof( NodeFlowPortAttribute ), false ) as NodeFlowPortAttribute[];
				foreach( var attr in flowPortAttrs )
				{
					NodeFlowPort port = CreateNodeFlowPort( false,
						Guid.NewGuid(), node, attr.IsInput,
						( null != flowPortViewModelTypeOverride ) ? flowPortViewModelTypeOverride : attr.ViewModelType,
						attr.Name, attr.DisplayName, attr.AllowMultipleInput, attr.AllowMultipleOutput, attr.IsPortEnabled, attr.IsEnabled );
				}

				//----- create nodePropertyPorts( property ) from NodePropertyAttribute.

				var propertyInfos = nodeType.GetProperties( BindingFlags.Public | BindingFlags.Instance );
				foreach( var propertyInfo in propertyInfos )
				{
					var nodePropertyAttrs = propertyInfo.GetCustomAttributes( typeof( NodePropertyPortAttribute ), false ) as NodePropertyPortAttribute[];
					if( null != nodePropertyAttrs )
					{
						foreach( var attr in nodePropertyAttrs )
						{
							NodePropertyPort port = CreateNodePropertyPort( false, Guid.NewGuid(), node, attr.IsInput, attr.ValueType, attr.DefaultValue, propertyInfo.Name,
								( null != propertyPortViewModelTypeOverride ) ? propertyPortViewModelTypeOverride : attr.ViewModelType,
								attr.DisplayName, attr.AllowMultipleInput, attr.AllowMultipleOutput, attr.IsPortEnabled, attr.IsEnabled );
						}
					}
				}

				//----- create nodePropertyPorts( field ) from NodePropertyAttribute.

				var fieldInfos = nodeType.GetFields( BindingFlags.Public | BindingFlags.Instance );
				foreach( var fieldInfo in fieldInfos )
				{
					var nodePropertyAttrs = fieldInfo.GetCustomAttributes( typeof( NodePropertyPortAttribute ), false ) as NodePropertyPortAttribute[];
					if( null != nodePropertyAttrs )
					{
						foreach( var attr in nodePropertyAttrs )
						{
							NodePropertyPort port = CreateNodePropertyPort( false, Guid.NewGuid(), node, attr.IsInput, attr.ValueType, attr.DefaultValue, fieldInfo.Name,
								( null != propertyPortViewModelTypeOverride ) ? propertyPortViewModelTypeOverride : attr.ViewModelType,
								attr.DisplayName, attr.AllowMultipleInput, attr.AllowMultipleOutput, attr.IsPortEnabled, attr.IsEnabled );
						}
					}
				}

				//----- invoke Create callback.

				node.OnCreate();
			}

			//----- return.

			return node;
		}

		public static void DestroyNode( Guid guid )
		{
			Node node;
			if( Nodes.TryGetValue( guid, out node ) )
			{
				//----- destroy.

				node.OnPreDestroy();

				List<Guid> connectorGuids = new List<Guid>();
				List<Guid> portGuids = new List<Guid>();

				foreach( var port in node.InputFlowPorts )
				{
					foreach( var connector in port.Connectors )
					{
						if( !connectorGuids.Contains( connector.Guid ) )
							connectorGuids.Add( connector.Guid );
					}
					portGuids.Add( port.Guid );
				}

				foreach( var port in node.OutputFlowPorts )
				{
					foreach( var connector in port.Connectors )
					{
						if( !connectorGuids.Contains( connector.Guid ) )
							connectorGuids.Add( connector.Guid );
					}
					portGuids.Add( port.Guid );
				}

				foreach( var port in node.InputPropertyPorts )
				{
					foreach( var connector in port.Connectors )
					{
						if( !connectorGuids.Contains( connector.Guid ) )
							connectorGuids.Add( connector.Guid );
					}
					portGuids.Add( port.Guid );
				}

				foreach( var port in node.OutputPropertyPorts )
				{
					foreach( var connector in port.Connectors )
					{
						if( !connectorGuids.Contains( connector.Guid ) )
							connectorGuids.Add( connector.Guid );
					}
					portGuids.Add( port.Guid );
				}

				foreach( var connectorGuid in connectorGuids )
				{
					DestroyConnector( connectorGuid );
				}

				foreach( var portGuid in portGuids )
				{
					DestroyNodePort( portGuid );
				}

				FlowChart flowChart = node.Owner;
				flowChart.ViewModel.NodeViewModels.Remove( node.ViewModel );
				flowChart.Nodes.Remove( node );

				ObservableCollection<Guid> selectionList = GetSelectionList( node.Owner );
				selectionList.Remove( guid );

				node.OnPostDestroy();

				node.Owner.History.AddCommand( new NodeGraph.History.DestroyNodeCommand(
					"Destroying node", SerializeNode( node ), node.Guid ) );

				Nodes.Remove( guid );
			}
		}

		public static Node FindNode( Guid guid )
		{
			Node node;
			Nodes.TryGetValue( guid, out node );
			return node;
		}

		public static List<Node> FindNode( FlowChart flowChart, string header )
		{
			List<Node> nodes = new List<Node>();

			foreach( var pair in Nodes )
			{
				Node node = pair.Value;
				if( ( flowChart == node.Owner ) && ( header == node.Header ) )
				{
					nodes.Add( node );
				}
			}

			return nodes;
		}

		#endregion // Node

		#region RouterNode

		public static Node CreateRouterNode( Guid guid, FlowChart flowChart, NodePort referencePort, double X, double Y, int ZIndex,
			Type nodeViewModelTypeOverride = null, Type flowPortViewModelTypeOverride = null, Type propertyPortViewModelTypeOverride = null )
		{
			if( null == flowChart )
				throw new ArgumentNullException( "flowChart of CreateNode() can not be null" );

			if( null == referencePort )
				throw new ArgumentNullException( "referencePort of CreateNode() can not be null" );

			Type portType = referencePort.GetType();
			bool isFlowPort = typeof( NodeFlowPort ).IsAssignableFrom( portType );
			bool isPropertyPort = typeof( NodePropertyPort ).IsAssignableFrom( portType );
			if( !isFlowPort && !isPropertyPort )
				throw new ArgumentException( "CreateRouterNode() is only supported for NodeFlowPort or NodePropertyPort" );

			Node node = CreateNode( false, guid, flowChart, typeof( Node ), X, Y, ZIndex,
				( null == nodeViewModelTypeOverride ) ? typeof( RouterNodeViewModel ) : nodeViewModelTypeOverride,
				flowPortViewModelTypeOverride, propertyPortViewModelTypeOverride );
			if( isFlowPort )
			{
				CreateNodeFlowPort( false, Guid.NewGuid(), node, true, flowPortViewModelTypeOverride, "Input", "", false, false, true, true );
				CreateNodeFlowPort( false, Guid.NewGuid(), node, false, flowPortViewModelTypeOverride, "Output", "", false, false, true, true );
			}
			else if( isPropertyPort )
			{
				NodePropertyPort propertyPort = referencePort as NodePropertyPort;
				CreateNodePropertyPort( false, Guid.NewGuid(), node, true, propertyPort.ValueType, propertyPort.Value, "Input",
					propertyPortViewModelTypeOverride,
					"", false, false, true, true );
				CreateNodePropertyPort( false, Guid.NewGuid(), node, false, propertyPort.ValueType, propertyPort.Value, "Output",
					propertyPortViewModelTypeOverride,
					"", false, false, true, true );
			}

			return node;
		}

		public static Node CreateRouterNodeForConnector( Guid guid, FlowChart flowChart, Connector connector, double X, double Y, int ZIndex )
		{
			NodePort startPort = connector.StartPort;
			NodePort endPort = connector.EndPort;

			DestroyConnector( connector.Guid );

			Node node = CreateRouterNode( guid, flowChart, startPort, X, Y, ZIndex );

			BeginConnection( startPort );
			if( startPort is NodeFlowPort )
			{
				EndConnection( node.InputFlowPorts[ 0 ] );

				BeginConnection( node.OutputFlowPorts[ 0 ] );
			}
			else
			{
				EndConnection( node.InputPropertyPorts[ 0 ] );

				BeginConnection( node.OutputPropertyPorts[ 0 ] );
			}

			EndConnection( endPort );

			return node;
		}

		#endregion // RouterNode

		#region Connector

		public static Connector CreateConnector( bool isDeserializing, Guid guid, FlowChart flowChart, Type connectorType = null )
		{
			//----- exceptions.

			if( null == flowChart )
				throw new ArgumentNullException( "flowChart of CreateNode() can not be null" );

			//------ create connector.

			if( null == connectorType )
				connectorType = typeof( Connector );

			var connectorAttrs = connectorType.GetCustomAttributes( typeof( ConnectorAttribute ), false ) as ConnectorAttribute[];
			if( 1 != connectorAttrs.Length )
				throw new ArgumentException( string.Format( "{0} must have ONE ConnectorAttribute", connectorType.Name ) );
			var connectorAttr = connectorAttrs[ 0 ];

			Connector connector = Activator.CreateInstance( connectorType, new object[] { guid, flowChart } ) as Connector;
			Connectors.Add( connector.Guid, connector );

			//----- create viewmodel

			connector.ViewModel = Activator.CreateInstance( connectorAttr.ViewModelType, new object[] { connector } ) as ConnectorViewModel;
			flowChart.ViewModel.ConnectorViewModels.Add( connector.ViewModel );
			flowChart.Connectors.Add( connector );

			//----- invoke Create callback.

			if( !isDeserializing )
			{
				connector.OnCreate();
			}

			//----- return.

			return connector;
		}

		public static void DestroyConnector( Guid guid )
		{
			Connector connector;
			if( Connectors.TryGetValue( guid, out connector ) )
			{
				//----- history.

				connector.FlowChart.History.AddCommand( new NodeGraph.History.DestroyConnectorCommand(
					"Destroying connector", SerializeConnector( connector ), connector.Guid ) );

				//----- destroy.

				connector.OnPreDestroy();

				if( null != connector.StartPort )
				{
					DisconnectFrom( connector.StartPort, connector );
				}

				if( null != connector.EndPort )
				{
					DisconnectFrom( connector.EndPort, connector );
				}

				FlowChart flowChart = connector.FlowChart;
				flowChart.ViewModel.ConnectorViewModels.Remove( connector.ViewModel );
				flowChart.Connectors.Remove( connector );

				connector.OnPostDestroy();
				Connectors.Remove( guid );
			}
		}

		public static Connector FindConnector( Guid guid )
		{
			Connector connector;
			Connectors.TryGetValue( guid, out connector );
			return connector;
		}

		#endregion // Connector

		#region Port

		public static NodePort FindNodePort( Guid guid )
		{
			NodePort port = FindNodeFlowPort( guid );
			if( null == port )
				port = FindNodePropertyPort( guid );
			return port;
		}

		public static void DestroyNodePort( Guid guid )
		{
			// ---- exception.

			NodePort port = FindNodePort( guid );
			if( null == port )
			{
				return;
			}

			//----- history.

			Node node = port.Owner;

			node.Owner.History.AddCommand( new History.DestroyNodePortCommand(
				"Destroying port", SerializeNodePort( port ), port.Guid ) );

			//----- destroy.

			bool isFlowPort = ( port is NodeFlowPort );

			port.OnPreDestroy();

			List<Guid> guids = new List<Guid>();
			foreach( var connector in port.Connectors )
			{
				guids.Add( connector.Guid );
			}

			foreach( var connectorGuid in guids )
			{
				DestroyConnector( connectorGuid );
			}

			if( port.IsInput )
			{
				node.ViewModel.InputFlowPortViewModels.Remove( port.ViewModel as NodeFlowPortViewModel );
				if( isFlowPort )
					node.InputFlowPorts.Remove( port as NodeFlowPort );
				else
					node.InputPropertyPorts.Remove( port as NodePropertyPort );
			}
			else
			{
				node.ViewModel.OutputFlowPortViewModels.Remove( port.ViewModel as NodeFlowPortViewModel );
				if( isFlowPort )
					node.OutputFlowPorts.Remove( port as NodeFlowPort );
				else
					node.OutputPropertyPorts.Remove( port as NodePropertyPort );
			}

			port.OnPostDestroy();

			if( isFlowPort )
				NodeFlowPorts.Remove( guid );
			else
				NodePropertyPorts.Remove( guid );
		}

		#endregion Port

		#region FlowPort

		/// <summary>
		/// Create NodeFlowPort with NodeFlwoPortViewModel.
		/// </summary>
		/// <param name="isDeserializing">Is in deserializing routine? 
		/// If it is true, OnCreate() callback will not be called, otherwise OnDeserialize will be called.</param>
		/// <param name="guid">Guid for this port.</param>
		/// <param name="node">Owner of this port.</param>
		/// <param name="name">Name of port.</param>
		/// <param name="displayName">Display name of port.</param>
		/// <param name="isInput">Is input port?</param>
		/// <param name="allowMultipleInput">Multiple inputs are allowed for this port?</param>
		/// <param name="allowMultipleOutput">Multiple outputs are allowed for this port?</param>
		/// <param name="portViewModelTypeOverride">ViewModelType to override.</param>
		/// <returns>Created NodeFlwoPort instance.</returns>
		public static NodeFlowPort CreateNodeFlowPort( bool isDeserializing, Guid guid, Node node, bool isInput, Type portViewModelTypeOverride = null,
			string name = "None", string displayName = "None", bool allowMultipleInput = true, bool allowMultipleOutput = false, bool isPortEnabled = true, bool isEnabled = true )
		{
			//----- exceptions.

			if( null == node )
				throw new ArgumentNullException( "node of CreateNodeFlowPort() can not be null" );

			//----- create port.

			// create flowPort model.
			NodeFlowPort port = Activator.CreateInstance( typeof( NodeFlowPort ),
				new object[] { guid, node, isInput } ) as NodeFlowPort;
			port.Name = name;
			port.DisplayName = displayName;
			port.AllowMultipleInput = allowMultipleInput;
			port.AllowMultipleOutput = allowMultipleOutput;
			port.IsPortEnabled = isPortEnabled;
			port.IsEnabled = isEnabled;
			NodeFlowPorts.Add( port.Guid, port );

			// create flowPort viewmodel.
			var portVM = Activator.CreateInstance( ( null != portViewModelTypeOverride ) ? portViewModelTypeOverride : typeof( NodeFlowPortViewModel ),
				new object[] { port } ) as NodeFlowPortViewModel;

			// add port to node.
			port.ViewModel = portVM;
			if( isInput )
			{
				node.InputFlowPorts.Add( port );
				node.ViewModel.InputFlowPortViewModels.Add( portVM );
			}
			else
			{
				node.OutputFlowPorts.Add( port );
				node.ViewModel.OutputFlowPortViewModels.Add( portVM );
			}

			//----- invoke Create callback.

			if( !isDeserializing )
			{
				port.OnCreate();
			}

			//----- history.

			node.Owner.History.AddCommand( new History.CreateNodePortCommand(
				"Creating port", port.Guid, SerializeNodePort( port ) ) );

			//----- return.

			return port;
		}

		public static NodeFlowPort FindNodeFlowPort( Guid guid )
		{
			NodeFlowPort port;
			NodeFlowPorts.TryGetValue( guid, out port );
			return port;
		}

		public static NodeFlowPort FindNodeFlowPort( Node node, string propertyName )
		{
			foreach( var pair in NodeFlowPorts )
			{
				NodeFlowPort port = pair.Value;
				if( ( node == port.Owner ) && ( propertyName == port.Name ) )
				{
					return port;
				}
			}

			return null;
		}

		#endregion // FlowPort

		#region PropertyPort

		/// <summary>
		/// Create PropertyPort with PropertyPortViewModel.
		/// </summary>
		/// <param name="isDeserializing">Is in deserializing routine? 
		/// If it is true, OnCreate() callback will not be called, otherwise OnDeserialize will be called.</param>
		/// <param name="guid">Guid for this port.</param>
		/// <param name="node">Owner of this port.</param>
		/// <param name="name">Name of port.</param>
		/// <param name="displayName">Display name of port.</param>
		/// <param name="isInput">Is input port?</param>
		/// <param name="allowMultipleInput">Multiple inputs are allowed for this port?</param>
		/// <param name="allowMultipleOutput">Multiple outputs are allowed for this port?</param>
		/// <param name="valueType">Type of property value.</param>
		/// <param name="defaultValue">Default property value.</param>
		// <param name="portViewModelTypeOverride">ViewModelType to override.</param>
		/// <returns>Created NodePropertyPort instance.</returns>
		public static NodePropertyPort CreateNodePropertyPort( bool isDeserializing, Guid guid, Node node, bool isInput, Type valueType, object defaultValue, string name,
			Type portViewModelTypeOverride = null, string displayName = "", bool allowMultipleInput = false, bool allowMultipleOutput = true, bool isPortEnabled = true, bool isEnabled = true )
		{
			//----- exceptions.

			if( null == node )
				throw new ArgumentNullException( "node of CreateNodePropertyPort() can not be null" );

			//----- create port.

			// create propertyPort model.
			NodePropertyPort port = Activator.CreateInstance( typeof( NodePropertyPort ),
				new object[] { guid, node, isInput, valueType, defaultValue, name } ) as NodePropertyPort;
			port.DisplayName = displayName;
			port.AllowMultipleInput = allowMultipleInput;
			port.AllowMultipleOutput = allowMultipleOutput;
			port.IsPortEnabled = isPortEnabled;
			port.IsEnabled = isEnabled;
			NodePropertyPorts.Add( port.Guid, port );

			// create propertyPort viewmodel.
			var portVM = Activator.CreateInstance( ( null != portViewModelTypeOverride ) ? portViewModelTypeOverride : typeof( NodePropertyPortViewModel ),
				new object[] { port } ) as NodePropertyPortViewModel;
			port.ViewModel = portVM;

			// add to node.
			if( port.IsInput )
			{
				node.InputPropertyPorts.Add( port );
				node.ViewModel.InputPropertyPortViewModels.Add( portVM );
			}
			else
			{
				node.OutputPropertyPorts.Add( port );
				node.ViewModel.OutputPropertyPortViewModels.Add( portVM );
			}

			//----- invoke Create callback.

			if( !isDeserializing )
			{
				port.OnCreate();
			}

			//----- history.

			node.Owner.History.AddCommand( new History.CreateNodePortCommand(
				"Creating port", port.Guid, SerializeNodePort( port ) ) );

			//----- return.

			return port;
		}

		public static NodePropertyPort FindNodePropertyPort( Guid guid )
		{
			NodePropertyPort port;
			NodePropertyPorts.TryGetValue( guid, out port );
			return port;
		}

		public static NodePropertyPort FindNodePropertyPort( Node node, string propertyName )
		{
			foreach( var pair in NodePropertyPorts )
			{
				NodePropertyPort port = pair.Value;
				if( ( node == port.Owner ) && ( propertyName == port.Name ) )
				{
					return port;
				}
			}

			return null;
		}

		#endregion // PropertyPort

		#region Connection

		public static bool IsConnecting { get; private set; }
		public static NodePort FirstConnectionPort { get; private set; }
		public static Connector CurrentConnector { get; private set; }

		public static void ConnectTo( NodePort port, Connector connector )
		{
			if( port.IsInput )
			{
				connector.EndPort = port;
			}
			else
			{
				connector.StartPort = port;
			}
			port.Connectors.Add( connector );

			port.OnConnect( connector );
			connector.OnConnect( port );
		}

		public static void DisconnectFrom( NodePort port, Connector connector )
		{
			if( null == port )
				return;

			connector.OnDisconnect( port );
			port.OnDisconnect( connector );

			if( port.IsInput )
			{
				connector.EndPort = null;
			}
			else
			{
				connector.StartPort = null;
			}
			port.Connectors.Remove( connector );
		}

		public static void DisconnectAll( NodePort port )
		{
			List<Guid> connectorGuids = new List<Guid>();
			foreach( var connection in port.Connectors )
			{
				connectorGuids.Add( connection.Guid );
			}

			foreach( var guid in connectorGuids )
			{
				DestroyConnector( guid );
			}
		}

		public static void BeginConnection( NodePort port )
		{
			if( IsConnecting )
				throw new InvalidOperationException( "You can not connect node during other connection occurs." );

			IsConnecting = true;

			Node node = port.Owner;
			FlowChart flowChart = node.Owner;
			FlowChartView flowChartView = flowChart.ViewModel.View;

			BeginDragging( flowChartView );

			CurrentConnector = CreateConnector( false, Guid.NewGuid(), flowChart, typeof( Connector ) );
			ConnectTo( port, CurrentConnector );

			FirstConnectionPort = port;
		}

		public static void SetOtherConnectionPort( NodePort port )
		{
			if( null == port )
			{
				if( ( null != CurrentConnector.StartPort ) && ( CurrentConnector.StartPort == FirstConnectionPort ) )
				{
					if( null != CurrentConnector.EndPort )
					{
						DisconnectFrom( CurrentConnector.EndPort, CurrentConnector );
					}
				}
				else if( ( null != CurrentConnector.EndPort ) && ( CurrentConnector.EndPort == FirstConnectionPort ) )
				{
					if( null != CurrentConnector.StartPort )
					{
						DisconnectFrom( CurrentConnector.StartPort, CurrentConnector );
					}
				}
			}
			else
			{
				ConnectTo( port, CurrentConnector );
			}
		}

		private static List<Node> _AlreadyCheckedNodes;

		public static bool CheckIfConnectable( NodePort otherPort, out string error )
		{
			Type firstType = FirstConnectionPort.GetType();
			Type otherType = otherPort.GetType();

			Node firstNode = FirstConnectionPort.Owner;
			Node otherNode = otherPort.Owner;

			error = "";

			// same port.
			if( FirstConnectionPort == otherPort )
			{
				//error = "It's a same port.";
				return false;
			}

			// same node.
			if( firstNode == otherNode )
			{
				error = "It's a port of same node.";
				return false;
			}

			bool areAllPropertyPorts = ( typeof( NodePropertyPort ).IsAssignableFrom( firstType ) && typeof( NodePropertyPort ).IsAssignableFrom( otherType ) );
			bool areAllFlowPorts = ( typeof( NodeFlowPort ).IsAssignableFrom( firstType ) && typeof( NodeFlowPort ).IsAssignableFrom( otherType ) );

			// different type of ports
			if( !areAllPropertyPorts && !areAllFlowPorts )
			{
				error = "Port type is not same with other's.";
				return false;
			}

			// same orientation.
			if( FirstConnectionPort.IsInput == otherPort.IsInput )
			{
				error = "Ports are all input or output.";
				return false;
			}

			// already connectecd.
			foreach( var connector in FirstConnectionPort.Connectors )
			{
				if( connector.StartPort == otherPort )
				{
					error = "Already connected";
					return false;
				}
			}

			// different type of value.
			if( areAllPropertyPorts )
			{
				NodePropertyPort firstPropPort = FirstConnectionPort as NodePropertyPort;
				NodePropertyPort otherPropPort = otherPort as NodePropertyPort;
				if( !firstPropPort.IsInput )
				{
					if( !otherPropPort.ValueType.IsAssignableFrom( firstPropPort.ValueType ) )
					{
						error = "Value type is not assignable";
						return false;
					}
				}
				else
				{
					if( !firstPropPort.ValueType.IsAssignableFrom( otherPropPort.ValueType ) )
					{
						error = "Value type is not assignable";
						return false;
					}
				}
			}

			// circular test
			if( !otherPort.Owner.AllowCircularConnection )
			{
				_AlreadyCheckedNodes = new List<Node>();
				if( IsReachable(
					FirstConnectionPort.IsInput ? firstNode : otherNode,
					FirstConnectionPort.IsInput ? otherNode : firstNode ) )
				{
					error = "Circular connection";
					_AlreadyCheckedNodes = null;
					return false;
				}
				_AlreadyCheckedNodes = null;
			}

			return FirstConnectionPort.IsConnectable( otherPort, out error );
		}

		private static bool IsReachable( Node nodeFrom, Node nodeTo )
		{
			if( _AlreadyCheckedNodes.Contains( nodeFrom ) )
				return false;

			_AlreadyCheckedNodes.Add( nodeFrom );

			foreach( var port in nodeFrom.OutputFlowPorts )
			{
				foreach( var connector in port.Connectors )
				{
					NodePort endPort = connector.EndPort;
					Node nextNode = endPort.Owner;
					if( nextNode == nodeTo )
						return true;

					if( IsReachable( nextNode, nodeTo ) )
						return true;
				}
			}

			foreach( var port in nodeFrom.OutputPropertyPorts )
			{
				foreach( var connector in port.Connectors )
				{
					NodePort endPort = connector.EndPort;
					Node nextNode = endPort.Owner;
					if( nextNode == nodeTo )
						return true;

					if( IsReachable( nextNode, nodeTo ) )
						return true;
				}
			}

			return false;
		}

		public static bool EndConnection( NodePort endPort = null )
		{
			EndDragging();

			bool bResult = false;

			if( !IsConnecting )
			{
				return false;
			}

			if( null != endPort )
			{
				SetOtherConnectionPort( endPort );
			}

			if( ( null == CurrentConnector.StartPort ) || ( null == CurrentConnector.EndPort ) )
			{
				DestroyConnector( CurrentConnector.Guid );
			}
			else
			{
				NodePort startPort = FindNodePort( CurrentConnector.StartPort.Guid );
				if( null == endPort )
					endPort = FindNodePort( CurrentConnector.EndPort.Guid );
				if( !startPort.AllowMultipleOutput )
				{
					List<Guid> connectorGuids = new List<Guid>();
					foreach( var connector in startPort.Connectors )
					{
						if( CurrentConnector.Guid != connector.Guid )
						{
							connectorGuids.Add( connector.Guid );
						}
					}

					foreach( var guid in connectorGuids )
					{
						DestroyConnector( guid );
					}
				}

				if( !endPort.AllowMultipleInput )
				{
					List<Guid> connectorGuids = new List<Guid>();
					foreach( var connector in endPort.Connectors )
					{
						if( CurrentConnector.Guid != connector.Guid )
						{
							connectorGuids.Add( connector.Guid );
						}
					}

					foreach( var guid in connectorGuids )
					{
						DestroyConnector( guid );
					}
				}

				//----- history.

				CurrentConnector.FlowChart.History.AddCommand( new History.CreateConnectorCommand(
					"Creating connector", CurrentConnector.Guid, SerializeConnector( CurrentConnector ) ) );

				bResult = true;
			}

			IsConnecting = false;
			CurrentConnector = null;
			FirstConnectionPort = null;

			return bResult;
		}

		public static void UpdateConnection( Point mousePos )
		{
			if( null != CurrentConnector )
				CurrentConnector.ViewModel.View.BuildCurveData( mousePos );
		}

		#endregion // Connection

		#region Node Dragging

		public static bool IsNodeDragging { get; private set; }
		public static bool AreNodesReallyDragged { get; private set; }
		private static Guid _NodeDraggingFlowChartGuid;

		public static void BeginDragNode( FlowChart flowChart )
		{
			BeginDragging( flowChart.ViewModel.View );

			if( IsNodeDragging )
				throw new InvalidOperationException( "Node is already being dragging." );

			IsNodeDragging = true;
			_NodeDraggingFlowChartGuid = flowChart.Guid;
		}

		public static void EndDragNode()
		{
			EndDragging();

			IsNodeDragging = false;
			AreNodesReallyDragged = false;
		}

		public static void DragNode( Point delta )
		{
			if( !IsNodeDragging )
				return;

			AreNodesReallyDragged = true;

			ObservableCollection<Guid> selectedNodes;
			if( SelectedNodes.TryGetValue( _NodeDraggingFlowChartGuid, out selectedNodes ) )
			{
				foreach( var guid in selectedNodes )
				{
					Node node = FindNode( guid );
					node.X += delta.X;
					node.Y += delta.Y;
				}
			}
		}

		#endregion // Node Dragging

		#region Mouse Trapping

		[DllImport( "user32.dll" )]
		public static extern void ClipCursor( ref System.Drawing.Rectangle rect );

		[DllImport( "user32.dll" )]
		public static extern void ClipCursor( IntPtr rect );

		private static FlowChartView _TrappingFlowChartView;
		public static bool IsDragging = false;

		public static void BeginDragging( FlowChartView flowChartView )
		{
			_TrappingFlowChartView = flowChartView;
			IsDragging = true;

			Point startLocation = flowChartView.PointToScreen( new Point( 0, 0 ) );

			System.Drawing.Rectangle rect = new System.Drawing.Rectangle(
				( int )startLocation.X, ( int )startLocation.Y,
				( int )( startLocation.X + flowChartView.ActualWidth ),
				( int )( startLocation.Y + flowChartView.ActualHeight ) );
			ClipCursor( ref rect );
		}

		public static void EndDragging()
		{
			if( null != _TrappingFlowChartView )
			{
				IsDragging = false;
				_TrappingFlowChartView = null;
			}

			ClipCursor( IntPtr.Zero );
		}

		#endregion // Mouse Trapping

		#region Node Selection

		public static Node MouseLeftDownNode { get; set; }

		public static ObservableCollection<Guid> GetSelectionList( FlowChart flowChart )
		{
			ObservableCollection<Guid> selectionList;
			if( !SelectedNodes.TryGetValue( flowChart.Guid, out selectionList ) )
				return null;
			return selectionList;
		}

		public static void TrySelection( FlowChart flowChart, Node node, bool bCtrl, bool bShift, bool bAlt )
		{
			bool bAdd = false;
			if( bCtrl )
			{
				bAdd = !node.ViewModel.IsSelected;
			}
			else if( bShift )
			{
				bAdd = true;
			}
			else if( bAlt )
			{
				bAdd = false;
			}
			else
			{
				DeselectAllNodes( flowChart );
				bAdd = true;
			}

			if( bAdd )
			{
				if( !node.ViewModel.IsSelected )
				{
					AddSelection( node );

					flowChart.History.AddCommand( new History.NodePropertyCommand(
						"Selection", node.Guid, "IsSelected", false, true ) );
				}
			}
			else
			{
				if( node.ViewModel.IsSelected )
				{
					RemoveSelection( node );

					flowChart.History.AddCommand( new History.NodePropertyCommand(
						"Selection", node.Guid, "IsSelected", true, false ) );
				}
			}
		}

		public static void AddSelection( Node node )
		{
			if( node.ViewModel.IsSelected )
			{
				return;
			}

			ObservableCollection<Guid> selectionList = GetSelectionList( node.Owner );
			if( !selectionList.Contains( node.Guid ) )
			{
				node.ViewModel.IsSelected = true;
				selectionList.Add( node.Guid );
			}

			MoveNodeToFront( node );
		}

		public static void RemoveSelection( Node node )
		{
			ObservableCollection<Guid> selectionList = GetSelectionList( node.Owner );
			node.ViewModel.IsSelected = false;
			selectionList.Remove( node.Guid );
		}

		public static void DeselectAllNodes( FlowChart flowChart )
		{
			ObservableCollection<Guid> selectionList = GetSelectionList( flowChart );

			foreach( var guid in selectionList )
			{
				Node node = FindNode( guid );
				node.ViewModel.IsSelected = false;

				flowChart.History.AddCommand( new History.NodePropertyCommand(
					"Deselection", node.Guid, "IsSelected", true, false ) );
			}
			selectionList.Clear();
		}

		public static void SelectAllNodes( FlowChart flowChart )
		{
			DeselectAllNodes( flowChart );

			ObservableCollection<Guid> selectionList = GetSelectionList( flowChart );
			foreach( var pair in Nodes )
			{
				Node node = pair.Value;
				if( node.Owner == flowChart )
				{
					node.ViewModel.IsSelected = true;
					selectionList.Add( node.Guid );
				}
			}
		}

		public static bool IsSelecting
		{
			get { return ( null != _FlowChartSelecting ); }
		}
		private static FlowChart _FlowChartSelecting;
		public static Point SelectingStartPoint { get; private set; }
		private static Guid[] _OriginalSelections;

		public static void BeginDragSelection( FlowChart flowChart, Point start )
		{
			FlowChartView flowChartView = flowChart.ViewModel.View;
			BeginDragging( flowChartView );

			SelectingStartPoint = start;

			_FlowChartSelecting = flowChart;
			_FlowChartSelecting.ViewModel.SelectionVisibility = Visibility.Visible;

			ObservableCollection<Guid> temp = new ObservableCollection<Guid>();
			SelectedNodes.TryGetValue( flowChart.Guid, out temp );
			_OriginalSelections = new Guid[ temp.Count ];
			temp.CopyTo( _OriginalSelections, 0 );
		}

		public static void UpdateDragSelection( FlowChart flowChart, Point end, bool bCtrl, bool bShift, bool bAlt )
		{
			FlowChartView flowChartView = flowChart.ViewModel.View;

			double startX = SelectingStartPoint.X;
			double startY = SelectingStartPoint.Y;

			Point selectionStart = new Point( Math.Min( startX, end.X ), Math.Min( startY, end.Y ) );
			Point selectionEnd = new Point( Math.Max( startX, end.X ), Math.Max( startY, end.Y ) );

			bool bAdd = false;
			if( bCtrl )
			{
				bAdd = true;
			}
			else if( bShift )
			{
				bAdd = true;
			}
			else if( bAlt )
			{
				bAdd = false;
			}
			else
			{
				bAdd = true;
			}

			foreach( var pair in Nodes )
			{
				Node node = pair.Value;
				if( node.Owner == _FlowChartSelecting )
				{
					Point nodeStart = new Point( node.X, node.Y );
					Point nodeEnd = new Point( node.X + node.ViewModel.View.ActualWidth,
						node.Y + node.ViewModel.View.ActualHeight );

					bool isInOriginalSelection = false;
					foreach( Guid nodeGuid in _OriginalSelections )
					{
						if( node.Guid == nodeGuid )
						{
							isInOriginalSelection = true;
							break;
						}
					}

					bool isOutside = ( nodeEnd.X < selectionStart.X ) ||
						( nodeEnd.Y < selectionStart.Y ) ||
						( nodeStart.X > selectionEnd.X ) ||
						( nodeStart.Y > selectionEnd.Y );

					bool isIncluded = !isOutside &&
						( nodeStart.X >= selectionStart.X ) &&
						( nodeStart.Y >= selectionStart.Y ) &&
						( nodeEnd.X <= selectionEnd.X ) &&
						( nodeEnd.Y <= selectionEnd.Y );

					bool isSelected = ( ( SelectionMode.Include == SelectionMode ) && isIncluded ) ||
						( ( SelectionMode.Overlap == SelectionMode ) && !isOutside );

					if( !isSelected )
					{
						if( isInOriginalSelection )
						{
							if( bCtrl || !bAdd )
							{
								AddSelection( node );
							}
						}
						else
						{
							if( bCtrl || bAdd )
							{
								RemoveSelection( node );
							}
						}

						continue;
					}

					bool bThisAdd = bAdd;
					if( isInOriginalSelection && bCtrl )
					{
						bThisAdd = false;
					}

					if( bThisAdd )
					{
						AddSelection( node );
					}
					else
					{
						RemoveSelection( node );
					}
				}
			}
		}

		public static bool EndDragSelection( bool bCancel )
		{
			EndDragging();

			bool bChanged = false;

			if( IsSelecting )
			{
				if( bCancel )
				{
					if( ( null != _FlowChartSelecting ) && ( null != _OriginalSelections ) )
					{
						DeselectAllNodes( _FlowChartSelecting );

						foreach( var guid in _OriginalSelections )
						{
							AddSelection( FindNode( guid ) );
						}
					}
				}
				else
				{
					if( null != _FlowChartSelecting )
					{
						ObservableCollection<Guid> selectionList = GetSelectionList( _FlowChartSelecting );
						foreach( var guid in _OriginalSelections )
						{
							if( !selectionList.Contains( guid ) )
							{
								_FlowChartSelecting.History.AddCommand( new History.NodePropertyCommand(
									"Selection", guid, "IsSelected", true, false ) );
								bChanged = true;
							}
						}

						foreach( var guid in selectionList )
						{
							if( -1 == Array.FindIndex( _OriginalSelections, ( currentGuid ) => guid == currentGuid ) )
							{
								_FlowChartSelecting.History.AddCommand( new History.NodePropertyCommand(
									"Selection", guid, "IsSelected", false, true ) );
								bChanged = true;
							}
						}
					}
				}

				if( null != _FlowChartSelecting )
				{
					_FlowChartSelecting.ViewModel.SelectionVisibility = Visibility.Collapsed;
				}
				_FlowChartSelecting = null;
				_OriginalSelections = null;
			}

			return bChanged;
		}

		#endregion // Node Selection

		#region Z-Indexing

		public static void MoveNodeToFront( Node node )
		{
			List<Node> nodes = new List<Node>();

			int maxZIndex = int.MinValue;
			foreach( var pair in Nodes )
			{
				Node currentNode = pair.Value;
				maxZIndex = Math.Max( maxZIndex, currentNode.ZIndex );
				nodes.Add( currentNode );
			}

			node.ZIndex = maxZIndex + 1;

			nodes.Sort( ( left, right ) => left.ZIndex.CompareTo( right.ZIndex ) );

			int zIndex = 0;
			foreach( var currentNode in nodes )
			{
				currentNode.ZIndex = zIndex++;
			}
		}

		#endregion // Z-Indexing.

		#region Delete

		public static void DestroySelectedNodes( FlowChart flowChart )
		{
			List<Guid> guids = new List<Guid>();

			ObservableCollection<Guid> selectedNodeGuids;
			SelectedNodes.TryGetValue( flowChart.Guid, out selectedNodeGuids );

			foreach( var guid in selectedNodeGuids )
			{
				guids.Add( guid );
			}

			foreach( var guid in guids )
			{
				DestroyNode( guid );
			}
		}

		#endregion // Delete

		#region ContentSize

		public static void CalculateContentSize( FlowChart flowChart, bool bOnlySelected,
			out double minX, out double maxX, out double minY, out double maxY )
		{
			minX = double.MaxValue;
			maxX = double.MinValue;
			minY = double.MaxValue;
			maxY = double.MinValue;

			bool hasNodes = false;
			foreach( var pair in Nodes )
			{
				Node node = pair.Value;
				NodeView nodeView = node.ViewModel.View;
				if( node.Owner == flowChart )
				{
					if( bOnlySelected && !node.ViewModel.IsSelected )
						continue;

					minX = Math.Min( node.X, minX );
					maxX = Math.Max( node.X + nodeView.ActualWidth, maxX );
					minY = Math.Min( node.Y, minY );
					maxY = Math.Max( node.Y + nodeView.ActualHeight, maxY );
					hasNodes = true;
				}
			}

			if( !hasNodes )
			{
				minX = maxX = minY = maxY = 0.0;
			}
		}

		#endregion // ContentSize

		#region Serialization

		private static XmlWriter CreateXmlWriter( StringWriter sw )
		{
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.IndentChars = "\t";
			settings.NewLineChars = "\n";
			settings.NewLineHandling = NewLineHandling.Replace;
			settings.NewLineOnAttributes = false;
			XmlWriter writer = XmlWriter.Create( sw, settings );
			return writer;
		}

		public static void Serialize( string filePath )
		{
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.IndentChars = "\t";
			settings.NewLineChars = "\n";
			settings.NewLineHandling = NewLineHandling.Replace;
			settings.NewLineOnAttributes = false;
			using( XmlWriter writer = XmlWriter.Create( filePath, settings ) )
			{
				writer.WriteStartDocument();
				{
					writer.WriteStartElement( "NodeGraphManager" );
					foreach( var pair in FlowCharts )
					{
						writer.WriteStartElement( "FlowChart" );
						pair.Value.WriteXml( writer );
						writer.WriteEndElement();
					}
					writer.WriteEndElement();
				}
				writer.WriteEndDocument();

				writer.Flush();
				writer.Close();
			}
		}

		public static bool Deserialize( string filePath )
		{
			if( !File.Exists( filePath ) )
			{
				return false;
			}

			List<FlowChart> loadedFlowCharts = new List<FlowChart>();

			using( XmlReader reader = XmlReader.Create( filePath ) )
			{
				while( reader.Read() )
				{
					if( XmlNodeType.Element == reader.NodeType )
					{
						if( "FlowChart" == reader.Name )
						{
							Guid guid = Guid.Parse( reader.GetAttribute( "Guid" ) );
							Type type = Type.GetType( reader.GetAttribute( "Type" ) );

							FlowChart flowChart = CreateFlowChart( true, guid, type );
							flowChart.ReadXml( reader );
							loadedFlowCharts.Add( flowChart );
						}
					}
				}
			}

			foreach( var flowChart in loadedFlowCharts )
			{
				flowChart.OnDeserialize();
			}

			return true;
		}

		public static string SerializeNode( Node node )
		{
			System.Text.StringBuilder builder = new System.Text.StringBuilder();
			StringWriter sw = new StringWriter( builder );
			XmlWriter writer = CreateXmlWriter( sw );

			writer.WriteStartElement( "Node" );
			node.WriteXml( writer );
			writer.WriteEndElement();

			sw.Flush();
			writer.Close();

			return builder.ToString();
		}

		public static void DeserializeNode( string xml )
		{
			XmlReader reader = XmlReader.Create( new StringReader( xml ) );
			while( reader.Read() )
			{
				if( XmlNodeType.Element == reader.NodeType )
				{
					if( "Node" == reader.Name )
					{
						Guid guid = Guid.Parse( reader.GetAttribute( "Guid" ) );
						Type type = Type.GetType( reader.GetAttribute( "Type" ) );
						FlowChart flowChart = FindFlowChart( Guid.Parse( reader.GetAttribute( "Owner" ) ) );
						Type vmType = Type.GetType( reader.GetAttribute( "ViewModelType" ) );

						Node node = CreateNode( true, guid, flowChart, type, 0.0, 0.0, 0, vmType );
						node.ReadXml( reader );

						node.OnDeserialize();

						break;
					}
				}
			}
		}

		public static string SerializeConnector( Connector connector )
		{
			System.Text.StringBuilder builder = new System.Text.StringBuilder();
			StringWriter sw = new StringWriter( builder );
			XmlWriter writer = CreateXmlWriter( sw );

			writer.WriteStartElement( "Connector" );
			connector.WriteXml( writer );
			writer.WriteEndElement();

			sw.Flush();
			writer.Close();

			return builder.ToString();
		}

		public static void DeserializeConnector( string xml )
		{
			XmlReader reader = XmlReader.Create( new StringReader( xml ) );
			while( reader.Read() )
			{
				if( XmlNodeType.Element == reader.NodeType )
				{
					if( "Connector" == reader.Name )
					{
						Guid guid = Guid.Parse( reader.GetAttribute( "Guid" ) );
						Type type = Type.GetType( reader.GetAttribute( "Type" ) );
						FlowChart flowChart = FindFlowChart( Guid.Parse( reader.GetAttribute( "Owner" ) ) );

						Connector connector = CreateConnector( true, guid, flowChart, type );
						connector.ReadXml( reader );

						connector.OnDeserialize();

						break;
					}
				}
			}
		}

		public static string SerializeNodePort( NodePort port )
		{
			System.Text.StringBuilder builder = new System.Text.StringBuilder();
			StringWriter sw = new StringWriter( builder );
			XmlWriter writer = CreateXmlWriter( sw );

			writer.WriteStartElement( "NodePort" );
			port.WriteXml( writer );
			writer.WriteEndElement();

			sw.Flush();
			writer.Close();

			return builder.ToString();
		}

		public static void DeserializeNodePort( string xml )
		{
			XmlReader reader = XmlReader.Create( new StringReader( xml ) );
			while( reader.Read() )
			{
				if( XmlNodeType.Element == reader.NodeType )
				{
					if( ( "NodePort" == reader.Name ) )
					{
						Guid guid = Guid.Parse( reader.GetAttribute( "Guid" ) );
						Type type = Type.GetType( reader.GetAttribute( "Type" ) );
						Type vmType = Type.GetType( reader.GetAttribute( "ViewModelType" ) );
						Node node = FindNode( Guid.Parse( reader.GetAttribute( "Owner" ) ) );
						bool isInput = bool.Parse( reader.GetAttribute( "IsInput" ) );

						bool isFlowPort = typeof( NodeFlowPort ).IsAssignableFrom( type );

						if( isFlowPort )
						{
							NodeFlowPort port = CreateNodeFlowPort(
								true, guid, node, isInput, vmType );
							port.ReadXml( reader );
							port.OnDeserialize();
						}
						else
						{
							string name = reader.GetAttribute( "Name" );
							Type valueType = Type.GetType( reader.GetAttribute( "ValueType" ) );

							NodePropertyPort port = CreateNodePropertyPort(
								true, guid, node, isInput, valueType, null, name, vmType );
							port.ReadXml( reader );
							port.OnDeserialize();
						}

						break;
					}
				}
			}
		}

		#endregion // Serialization

		#region ContextMenu

		public delegate bool BuildContextMenuDelegate( object sender, BuildContextMenuArgs args );
		public static event BuildContextMenuDelegate BuildFlowChartContextMenu;
		public static event BuildContextMenuDelegate BuildNodeContextMenu;
		public static event BuildContextMenuDelegate BuildFlowPortContextMenu;
		public static event BuildContextMenuDelegate BuildPropertyPortContextMenu;

		public static bool InvokeBuildContextMenu( object sender, BuildContextMenuArgs args )
		{
			BuildContextMenuDelegate targetEvent = null;

			switch( args.ModelType )
			{
				case ModelType.FlowChart:
					targetEvent = BuildFlowChartContextMenu;
					break;
				case ModelType.Node:
					targetEvent = BuildNodeContextMenu;
					break;
				case ModelType.FlowPort:
					targetEvent = BuildFlowPortContextMenu;
					break;
				case ModelType.PropertyPort:
					targetEvent = BuildPropertyPortContextMenu;
					break;
			}

			if( null == targetEvent )
				return false;

			return targetEvent.Invoke( sender, args );
		}

		#endregion // ContextMenu

		#region Selection Events

		public delegate void NodeSelectionChangedDelegate( FlowChart flowChart, ObservableCollection<Guid> nodes, NotifyCollectionChangedEventArgs args );
		public static event NodeSelectionChangedDelegate NodeSelectionChanged;

		private static void Node_SelectionList_CollectionChanged( object sender, NotifyCollectionChangedEventArgs args )
		{
			FlowChart flowChart = null;
			foreach( var pair in SelectedNodes )
			{
				if( pair.Value == sender )
				{
					flowChart = FindFlowChart( pair.Key );
				}
			}

			NodeSelectionChanged?.Invoke( flowChart, sender as ObservableCollection<Guid>, args );
		}

		#endregion // Selection Events

		#region Drag & Drop Events

		public delegate void NodeGraphDragEventDelegate( object sender, NodeGraphDragEventArgs args );

		public static event NodeGraphDragEventDelegate DragEnter;
		public static event NodeGraphDragEventDelegate DragLeave;
		public static event NodeGraphDragEventDelegate DragOver;
		public static event NodeGraphDragEventDelegate Drop;

		public static void InvokeDragEnter( object sender, NodeGraphDragEventArgs args )
		{
			DragEnter?.Invoke( sender, args );
		}

		public static void InvokeDragLeave( object sender, NodeGraphDragEventArgs args )
		{
			DragLeave?.Invoke( sender, args );
		}

		public static void InvokeDragOver( object sender, NodeGraphDragEventArgs args )
		{
			DragOver?.Invoke( sender, args );
		}

		public static void InvokeDrop( object sender, NodeGraphDragEventArgs args )
		{
			Drop?.Invoke( sender, args );
		}

		#endregion // Drag & Drop Events

		#region Logs

		public static void AddScreenLog( FlowChart flowChart, string log )
		{
			FlowChartView view = flowChart.ViewModel.View;
			view.AddLog( log );
		}

		public static void RemoveScreenLog( FlowChart flowChart, string log )
		{
			FlowChartView view = flowChart.ViewModel.View;
			view.RemoveLog( log );
		}

		public static void ClearScreenLogs( FlowChart flowChart )
		{
			FlowChartView view = flowChart.ViewModel.View;
			view.ClearLogs();
		}

		#endregion // Logs
	}

	public enum ModelType
	{
		FlowChart,
		Node,
		FlowPort,
		PropertyPort,
	}

	public class BuildContextMenuArgs
	{
		public Point ViewSpaceMouseLocation { get; set; }
		public Point ModelSpaceMouseLocation { get; set; }
		public ModelType ModelType { get; set; }
		public System.Windows.Controls.ContextMenu ContextMenu { get; internal set; }
	}

	public class NodeGraphDragEventArgs
	{
		public Point ViewSpaceMouseLocation { get; set; }
		public Point ModelSpaceMouseLocation { get; set; }
		public ModelType ModelType { get; set; }
		public DragEventArgs DragEventArgs { get; set; }
	}
}
