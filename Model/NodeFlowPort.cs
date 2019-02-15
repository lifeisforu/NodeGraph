using NodeGraph.ViewModel;
using System;

namespace NodeGraph.Model
{
	public class NodeFlowPort : NodePort
	{
		#region Properties

		#endregion // Properties

		#region Constructor

		/// <summary>
		/// Never call this constructor directly. Use GraphManager.CreateNodeFlowPort() method.
		/// </summary>
		public NodeFlowPort( Guid guid, Node node, string name, string displayName, bool isInput, bool allowMultipleInput ) :
			base( guid, node, name, displayName, isInput, allowMultipleInput )
		{
			
		}

		#endregion // Constructor
	}
}
