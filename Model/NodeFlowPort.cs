using NodeGraph.ViewModel;
using System;

namespace NodeGraph.Model
{
	public class NodeFlowPort : NodePort
	{
		#region Properties

		#endregion // Properties

		#region Constructor

		public NodeFlowPort( Guid guid, Node node, NodeFlowPortAttribute attr ) : base( guid, node, attr )
		{
			
		}

		#endregion // Constructor
	}
}
