using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeGraph.ViewModel
{
	[AttributeUsage( AttributeTargets.Class )]
	public class NodePropertyPortViewModelAttribute : Attribute
	{
		public string ViewStyleName;
		public NodePropertyPortViewModelAttribute( string viewStyleName = "DefaultNodePropertyPortViewStyle" )
		{
			ViewStyleName = viewStyleName;
		}
	}
}
