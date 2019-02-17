using NodeGraph.Model;
using NodeGraph.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeGraph.ViewModel
{
	[NodePropertyPortViewModel()]
	public class NodePropertyPortViewModel : NodePortViewModel
	{
		#region Constructor

		public NodePropertyPortViewModel( NodePropertyPort nodeProperty ) : base( nodeProperty )
		{
			Model = nodeProperty;
		}

		#endregion // Constructor
	}
}
