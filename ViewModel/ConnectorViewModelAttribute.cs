using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectorGraph.ViewModel
{
	[AttributeUsage( AttributeTargets.Class )]
	public class ConnectorViewModelAttribute : Attribute
	{
		public string ViewStyleName;
		public ConnectorViewModelAttribute( string viewStyleName = "DefaultConnectorViewStyle" )
		{
			ViewStyleName = viewStyleName;
		}
	}
}
