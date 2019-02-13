using NodeGraph.Model;
using NodeGraph.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeGraph.ViewModel
{
	public class NodeFlowPortViewModel : NodePortViewModel
	{
		#region Constructor

		public NodeFlowPortViewModel( NodeFlowPort nodeFlowPort ) : base( nodeFlowPort )
		{
		}

		#endregion // Constructor

	}
}
