using NodeGraph.ViewModel;
using System;
using System.Xml;

namespace NodeGraph.Model
{
	public class NodeFlowPort : NodePort
	{
		#region Constructor

		/// <summary>
		/// Never call this constructor directly. Use GraphManager.CreateNodeFlowPort() method.
		/// </summary>
		public NodeFlowPort( Guid guid, Node node, bool isInput ) :
			base( guid, node, isInput )
		{
		}

		#endregion // Constructor
	}
}
