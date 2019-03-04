using NodeGraph.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeGraph.ViewModel
{
	[AttributeUsage( AttributeTargets.Class )]
	public class NodeFlowPortViewModelAttribute : Attribute
	{
		public string ViewStyleName = "DefaultNodeFlowPortViewStyle";
		public Type ViewType = typeof( NodeFlowPortView );
	}
}
