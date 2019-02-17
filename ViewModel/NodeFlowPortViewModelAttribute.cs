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
		public string ViewStyleName;
		public NodeFlowPortViewModelAttribute( string viewStyleName = "DefaultNodeFlowPortViewStyle" )
		{
			ViewStyleName = viewStyleName;
		}
	}
}
