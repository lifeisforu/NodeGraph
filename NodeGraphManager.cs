using NodeGraph.Model;
using NodeGraph.ViewModel;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Media;

namespace NodeGraph
{
	public class NodeGraphManager
	{
		#region Instance

		public static NodeGraphManager This = new NodeGraphManager();

		private NodeGraphManager() { }

		#endregion // Instance

		#region Fields

		public readonly Dictionary<Guid, FlowChart> FlowCharts = new Dictionary<Guid, FlowChart>();
		public readonly Dictionary<Guid, Node> Nodes = new Dictionary<Guid, Node>();
		public readonly Dictionary<Guid, Connector> Connectors = new Dictionary<Guid, Connector>();
		public readonly Dictionary<Guid, NodeFlowPort> NodeFlowPorts = new Dictionary<Guid, NodeFlowPort>();
		public readonly Dictionary<Guid, NodePropertyPort> NodePropertyPorts = new Dictionary<Guid, NodePropertyPort>();
		public readonly Dictionary<Guid, List<Guid>> SelectedNodes = new Dictionary<Guid, List<Guid>>();

		#endregion // Fields

		#region FlowChart

		public FlowChart CreateFlowChart( Guid guid, Type flowChartModelType )
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

			SelectedNodes.Add( flowChart.Guid, new List<Guid>() );

			flowChart.InvokeCreateEvent();

			//----- return.

			return flowChart;
		}

		public void DestroyFlowChart( Guid guid )
		{
			List<Guid> guids = new List<Guid>();

			foreach( var pair in Nodes )
			{
				Node node = pair.Value;
				if( guid == node.Owner.Guid )
				{
					guids.Add( node.Guid );
				}
			}

			foreach( var nodeGuid in guids )
			{
				DestroyNode( nodeGuid );
			}

			guids = new List<Guid>();

			foreach( var pair in Connectors )
			{
				Connector connector = pair.Value;
				if( guid == connector.Owner.Guid )
				{
					guids.Add( connector.Guid );
				}
			}

			foreach( var connectorGuid in guids )
			{
				DestroyConnector( connectorGuid );
			}

			FlowCharts.Remove( guid );
		}

		public FlowChart FindFlowChart( Guid guid )
		{
			FlowChart flowChart;
			FlowCharts.TryGetValue( guid, out flowChart );
			return flowChart;
		}

		#endregion // FlowChart

		#region Node

		public Node CreateNode( Guid guid, FlowChart flowChart, Type nodeType )
		{
			//----- create node from NodeAttribute.

			var nodeAttrs = nodeType.GetCustomAttributes( typeof( NodeAttribute ), false ) as NodeAttribute[];
			if( 1 != nodeAttrs.Length )
				throw new ArgumentException( string.Format( "{0} must have ONE NodeAttribute", nodeType.Name ) );
			var nodeAttr = nodeAttrs[ 0 ];

			// create node model.
			Node node = Activator.CreateInstance( nodeType, new object[]{ guid, flowChart, nodeAttr.AllowCircularConnection } ) as Node;
			Nodes.Add( guid, node );
			// create node viewmodel.
			node.ViewModel = Activator.CreateInstance( nodeAttr.ViewModelType, new object[] { node } ) as NodeViewModel;
			flowChart.ViewModel.NodeViewModels.Add( node.ViewModel );
			flowChart.Nodes.Add( node );

			node.Header = nodeAttr.Header;
			node.HeaderBackgroundColor = new SolidColorBrush( ( Color )ColorConverter.ConvertFromString( nodeAttr.HeaderBackgroundColor ) );
			node.HeaderFontColor = new SolidColorBrush( ( Color )ColorConverter.ConvertFromString( nodeAttr.HeaderFontColor ) );

			//----- create flowPorts from NodeFlowPortAttribute.

			var flowPortAttrs = nodeType.GetCustomAttributes( typeof( NodeFlowPortAttribute ), false ) as NodeFlowPortAttribute[];
			foreach( var attr in flowPortAttrs )
			{
				NodeFlowPort port = CreateNodeFlowPort( 
					Guid.NewGuid(), node, attr.Name, attr.DisplayName, attr.IsInput, attr.AllowMultipleInput, attr.ViewModelType );
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
						NodePropertyPort port = CreateNodePropertyPort( Guid.NewGuid(), node,
							propertyInfo.Name, attr.DisplayName, attr.IsInput, attr.AllowMultipleInput, attr.Type, attr.DefaultValue, attr.ViewModelType );
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
						NodePropertyPort port = CreateNodePropertyPort( Guid.NewGuid(), node,
							fieldInfo.Name, attr.DisplayName, attr.IsInput, attr.AllowMultipleInput, attr.Type, attr.DefaultValue, attr.ViewModelType );
					}
				}
			}

			//----- invoke Create event.

			node.InvokeCreateEvent();

			//----- return.

			return node;
		}

		public void DestroyNode( Guid guid )
		{
			Node node;
			if( Nodes.TryGetValue( guid, out node ) )
			{
				List<Guid> guids = new List<Guid>();

				foreach( var port in node.InputFlowPorts )
				{
					guids.Add( port.Guid );
				}

				foreach( var port in node.OutputFlowPorts )
				{
					guids.Add( port.Guid );
				}

				foreach( var portGuid in guids )
				{
					DestroyNodeFlowPort( portGuid );
				}

				guids = new List<Guid>();

				foreach( var port in node.InputPropertyPorts )
				{
					guids.Add( port.Guid );
				}

				foreach( var port in node.OutputPropertyPorts )
				{
					guids.Add( port.Guid );
				}

				foreach( var portGuid in guids )
				{
					DestroyNodePropertyPort( portGuid );
				}

				FlowChart flowChart = node.Owner;
				flowChart.ViewModel.NodeViewModels.Remove( node.ViewModel );
				flowChart.Nodes.Remove( node );

				List<Guid> selectionList;
				SelectedNodes.TryGetValue( node.Owner.Guid, out selectionList );
				selectionList.RemoveAll( ( currentGuid ) => currentGuid == guid );

				Nodes.Remove( guid );
			}
		}

		public Node FindNode( Guid guid )
		{
			Node node;
			Nodes.TryGetValue( guid, out node );
			return node;
		}

		#endregion // Node

		#region Connector

		public Connector CreateConnector( Guid guid, FlowChart flowChart, Type connectorType )
		{
			//------ create connector.

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

			connector.InvokeCreateEvent();

			//----- return.

			return connector;
		}

		public void DestroyConnector( Guid guid )
		{
			Connector connector;
			if( Connectors.TryGetValue( guid, out connector ) )
			{
				if( null != connector.StartPort )
				{
					connector.StartPort.Connectors.Remove( connector );
					connector.StartPort = null;
				}

				if( null != connector.EndPort )
				{
					connector.EndPort.Connectors.Remove( connector );
					connector.EndPort = null;
				}

				FlowChart flowChart = connector.Owner;
				flowChart.ViewModel.ConnectorViewModels.Remove( connector.ViewModel );
				flowChart.Connectors.Remove( connector );
				Connectors.Remove( guid );
			}
		}

		public Connector FindConnector( Guid guid )
		{
			Connector connector;
			Connectors.TryGetValue( guid, out connector );
			return connector;
		}

		#endregion // Connector

		#region FlowPort

		public NodeFlowPort CreateNodeFlowPort( Guid guid, Node node, string name, string displayName, bool isInput, bool allowMultipleInput, Type viewModelType = null )
		{
			if( null == node )
				throw new ArgumentNullException( "owner cannot be null in CreateNodeFlowPort()" );

			// create flowPort model.
			NodeFlowPort port = Activator.CreateInstance( typeof( NodeFlowPort ),
				new object[] { guid, node, name, displayName, isInput, allowMultipleInput } ) as NodeFlowPort;
			NodeFlowPorts.Add( port.Guid, port );
			
			// create flowPort viewmodel.
			var portVM = Activator.CreateInstance( ( null != viewModelType ) ? viewModelType : typeof( NodeFlowPortViewModel ),
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

			port.InvokeCreateEvent();

			return port;
		}

		public NodeFlowPort FindNodeFlowPort( Guid guid )
		{
			NodeFlowPort port;
			NodeFlowPorts.TryGetValue( guid, out port );
			return port;
		}

		public void DestroyNodeFlowPort( Guid guid )
		{
			NodeFlowPort port = FindNodeFlowPort( guid );
			Node node = port.Owner;

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
				node.InputFlowPorts.Remove( port );
			}
			else
			{
				node.ViewModel.OutputFlowPortViewModels.Remove( port.ViewModel as NodeFlowPortViewModel );
				node.OutputFlowPorts.Remove( port );
			}

			NodeFlowPorts.Remove( guid );
		}

		#endregion // FlowPort

		#region PropertyPort

		public NodePropertyPort CreateNodePropertyPort( Guid guid, Node node, string name, string displayName, bool isInput, bool allowMultipleInput, Type valueType, object defaultValue, Type viewModelType = null )
		{
			if( null == node )
				throw new ArgumentNullException( "owner cannot be null in CreateNodeFlowPort()" );

			// create propertyPort model.
			NodePropertyPort port = Activator.CreateInstance( typeof( NodePropertyPort ), new object[]{ guid, node,
							name, displayName, isInput, allowMultipleInput, valueType, defaultValue } ) as NodePropertyPort;
			NodePropertyPorts.Add( port.Guid, port );

			// create propertyPort viewmodel.
			var portVM = Activator.CreateInstance( ( null != viewModelType ) ? viewModelType : typeof( NodePropertyPortViewModel ),
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

			port.InvokeCreateEvent();

			return port;
		}

		public NodePropertyPort FindNodePropertyPort( Guid guid )
		{
			NodePropertyPort port;
			NodePropertyPorts.TryGetValue( guid, out port );
			return port;
		}

		public void DestroyNodePropertyPort( Guid guid )
		{
			NodePropertyPort port = FindNodePropertyPort( guid );
			Node node = port.Owner;

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
				node.ViewModel.InputPropertyPortViewModels.Remove( port.ViewModel as NodePropertyPortViewModel );
				node.InputPropertyPorts.Remove( port );
			}
			else
			{
				node.ViewModel.OutputPropertyPortViewModels.Remove( port.ViewModel as NodePropertyPortViewModel );
				node.OutputPropertyPorts.Remove( port );
			}

			NodePropertyPorts.Remove( guid );
		}

		#endregion // PropertyPort

		#region Connection

		public bool IsConnecting{ get; private set; }
		private Connector _Connector;
		public NodePort _FirstPort;

		public void BeginConnection( NodePort port )
		{
			if( IsConnecting )
				throw new InvalidOperationException( "You can not connect node during other connection occurs." );

			IsConnecting = true;

			Node node = port.Owner;
			FlowChart flowChart = node.Owner;

			_Connector = CreateConnector( Guid.NewGuid(), flowChart, typeof( Connector ) );

			if( port.IsInput )
			{
				_Connector.EndPort = port;
			}
			else
			{
				_Connector.StartPort = port;
			}
			port.Connectors.Add( _Connector );

			_FirstPort = port;
		}

		public void SetOtherConnectionPort( NodePort port )
		{
			if( null == port )
			{
				if( ( null != _Connector.StartPort ) && ( _Connector.StartPort == _FirstPort ) )
				{
					if( null != _Connector.EndPort )
					{
						_Connector.EndPort.Connectors.Remove( _Connector );
						_Connector.EndPort = null;
					}
				}
				else if( ( null != _Connector.EndPort ) && ( _Connector.EndPort == _FirstPort ) )
				{
					if( null != _Connector.StartPort )
					{
						_Connector.StartPort.Connectors.Remove( _Connector );
						_Connector.StartPort = null;
					}
				}
			}
			else
			{
				if( port.IsInput )
				{
					_Connector.EndPort = port;
				}
				else
				{
					_Connector.StartPort = port;
				}

				port.Connectors.Add( _Connector );
			}
		}

		private List<Node> _AlreadyCheckedNodes;

		public bool CheckIfConnectable( NodePort otherPort )
		{
			Type firstType = _FirstPort.GetType();
			Type otherType = otherPort.GetType();

			Node firstNode = _FirstPort.Owner;
			Node otherNode = otherPort.Owner;

			// same port.
			if( _FirstPort == otherPort )
			{
				return false;
			}

			// same node.
			if( firstNode == otherNode )
			{
				return false;
			}

			bool areAllPropertyPorts = ( typeof( NodePropertyPort ).IsAssignableFrom( firstType ) && typeof( NodePropertyPort ).IsAssignableFrom( otherType ) );
			bool areAllFlowPorts = ( typeof( NodeFlowPort ).IsAssignableFrom( firstType ) && typeof( NodeFlowPort ).IsAssignableFrom( otherType ) );

			// different type of ports
			if( !areAllPropertyPorts && !areAllFlowPorts )
			{
				return false;
			}

			// same orientation.
			if( _FirstPort.IsInput == otherPort.IsInput )
			{
				return false;
			}

			// already connectecd.
			foreach( var connector in _FirstPort.Connectors )
			{
				if( connector.StartPort == otherPort )
				{
					return false;
				}
			}

			// different type of value.
			if( areAllPropertyPorts )
			{
				NodePropertyPort firstPropPort = _FirstPort as NodePropertyPort;
				NodePropertyPort otherPropPort = otherPort as NodePropertyPort;

				if( firstPropPort.TypeOfValue != otherPropPort.TypeOfValue )
				{
					return false;
				}
			}

			// circular test
			if( !otherPort.Owner.AllowCircularConnection )
			{
				_AlreadyCheckedNodes = new List<Node>();
				if( IsReachable(
					_FirstPort.IsInput ? firstNode : otherNode, 
					_FirstPort.IsInput ? otherNode : firstNode ) )
				{
					_AlreadyCheckedNodes = null;
					return false;
				}
				_AlreadyCheckedNodes = null;
			}

			return true;
		}

		private bool IsReachable( Node nodeFrom, Node nodeTo )
		{
			if( _AlreadyCheckedNodes.Contains( nodeFrom ) )
				return false;

			_AlreadyCheckedNodes.Add( nodeFrom );

			foreach( var port in nodeFrom.OutputFlowPorts )
			{
				foreach( var connector in port.Connectors )
				{
					Node nextNode = connector.EndPort.Owner;
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
					Node nextNode = connector.EndPort.Owner;
					if( nextNode == nodeTo )
						return true;

					if( IsReachable( nextNode, nodeTo ) )
						return true;
				}
			}

			return false;
		}

		public void EndConnection()
		{
			if( !IsConnecting )
				return;

			if( ( null == _Connector.StartPort ) || ( null == _Connector.EndPort ) )
			{
				DestroyConnector( _Connector.Guid );
			}
			else
			{
				if( !_Connector.EndPort.AllowMultipleInput )
				{
					List<Guid> connectorGuids = new List<Guid>();
					foreach( var connector in _Connector.EndPort.Connectors )
					{
						if( _Connector.Guid != connector.Guid )
						{
							connectorGuids.Add( connector.Guid );
						}
					}

					foreach( var guid in connectorGuids )
					{
						DestroyConnector( guid );
					}
				}
			}

			IsConnecting = false;
			_Connector = null;
			_FirstPort = null;
		}

		public void UpdateConnection()
		{
			if( null != _Connector )
				_Connector.ViewModel.View.BuildCurveData();
		}

		public void Disconnect( NodePort port )
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

		#endregion // Connection

		#region Node Dragging

		public bool IsNodeDragging { get; private set; }
		private Node _NodeDragging;

		public void StartDragNode( Node node )
		{
			if( IsNodeDragging )
				throw new InvalidOperationException( "Node is already being dragging." );

			IsNodeDragging = true;
			_NodeDragging = node;
		}

		public void EndDragNode()
		{
			_NodeDragging = null;
			IsNodeDragging = false;
		}

		public void DragNode( Point offset )
		{
			_NodeDragging.X += offset.X;
			_NodeDragging.Y += offset.Y;
		}

		#endregion // Node Dragging

		#region Node Selection

		public void TrySelection( Node node, bool bCtrl, bool bShift, bool bAlt )
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
				DeslectAllNodes( node.Owner );
				bAdd = true;
			}

			if( bAdd )
				AddSelection( node );
			else
				RemoveSelection( node );
		}

		public void AddSelection( Node node )
		{
			FlowChart flowChart = node.Owner;
			List<Guid> selectionList;
			SelectedNodes.TryGetValue( flowChart.Guid, out selectionList );

			if( !selectionList.Contains( node.Guid ) )
			{
				node.ViewModel.IsSelected = true;
				selectionList.Add( node.Guid );
			}
		}

		public void RemoveSelection( Node node )
		{
			FlowChart flowChart = node.Owner;
			List<Guid> selectionList;
			SelectedNodes.TryGetValue( flowChart.Guid, out selectionList );

			node.ViewModel.IsSelected = false;
			selectionList.Remove( node.Guid );
		}

		public void DeslectAllNodes( FlowChart flowChart )
		{
			List<Guid> selectionList;
			SelectedNodes.TryGetValue( flowChart.Guid, out selectionList );
			foreach( var guid in selectionList )
			{
				Node node = FindNode( guid );
				node.ViewModel.IsSelected = false;
			}
			selectionList.Clear();
		}

		public void SelectAllNodes( FlowChart flowChart )
		{
			DeslectAllNodes( flowChart );

			List<Guid> selectionList;
			SelectedNodes.TryGetValue( flowChart.Guid, out selectionList );
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

		public bool IsSelecting
		{
			get { return ( null != _FlowChartSelecting ); }
		}
		private FlowChart _FlowChartSelecting;
		private Point _SelectingStartPoint;
		private Guid[] _OriginalSelections;

		public void StartDragSelection( FlowChart flowChart, Point start )
		{
			_SelectingStartPoint = start;

			_FlowChartSelecting = flowChart;
			_FlowChartSelecting.ViewModel.SelectionStartX = start.X;
			_FlowChartSelecting.ViewModel.SelectionWidth = 0;
			_FlowChartSelecting.ViewModel.SelectionStartY = start.Y;
			_FlowChartSelecting.ViewModel.SelectionHeight = 0;
			_FlowChartSelecting.ViewModel.SelectionVisibility = Visibility.Visible;

			List<Guid> temp = new List<Guid>();
			SelectedNodes.TryGetValue( flowChart.Guid, out temp );
			_OriginalSelections = new Guid[ temp.Count ];
			temp.CopyTo( _OriginalSelections );
		}

		public void UpdateDragSelection( Point end, bool bCtrl, bool bShift, bool bAlt )
		{
			double startX = _SelectingStartPoint.X;
			double startY = _SelectingStartPoint.Y;

			Point realStart = new Point( Math.Min( startX, end.X ), Math.Min( startY, end.Y ) );
			Point realEnd = new Point( Math.Max( startX, end.X ), Math.Max( startY, end.Y ) );

			_FlowChartSelecting.ViewModel.SelectionStartX = realStart.X;
			_FlowChartSelecting.ViewModel.SelectionStartY = realStart.Y;
			_FlowChartSelecting.ViewModel.SelectionWidth = realEnd.X - realStart.X;
			_FlowChartSelecting.ViewModel.SelectionHeight = realEnd.Y - realStart.Y;

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
					double nodeStartX = node.X;
					double nodeEndX = node.X + node.ViewModel.View.ActualWidth;
					double nodeStartY = node.Y;
					double nodeEndY = node.Y + node.ViewModel.View.ActualHeight;

					bool isInOriginalSelection = false;
					foreach( Guid nodeGuid in _OriginalSelections )
					{
						if( node.Guid == nodeGuid )
						{
							isInOriginalSelection = true;
							break;
						}
					}

					// outside.
					if( ( nodeEndX < realStart.X ) ||
						( nodeEndY < realStart.Y ) ||
						( nodeStartX > realEnd.X ) ||
						( nodeStartY > realEnd.Y ) )
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

		public void EndDragSelection( bool bCancel )
		{
			if( bCancel )
			{
				if( ( null != _FlowChartSelecting ) && ( null != _OriginalSelections ) )
				{
					DeslectAllNodes( _FlowChartSelecting );

					foreach( var guid in _OriginalSelections )
					{
						AddSelection( FindNode( guid ) );
					}
				}
			}

			if( null != _FlowChartSelecting )
				_FlowChartSelecting.ViewModel.SelectionVisibility = Visibility.Collapsed;
			_FlowChartSelecting = null;
			_OriginalSelections = null;
		}

		#endregion // Node Selection

		#region Delete

		public void DestroySelectedNodes( FlowChart flowChart )
		{
			List<Guid> guids = new List<Guid>();

			List<Guid> selectedNodeGuids;
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
	}
}
