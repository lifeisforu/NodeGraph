using NodeGraph.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeGraph.ViewModel
{
	[NodeViewModel( ViewStyleName = "RouterNodeViewStyle" )]
	public class RouterNodeViewModel : NodeViewModel
	{
		#region Constructor

		public RouterNodeViewModel( Node node ) : base( node )
		{

		}

		#endregion // Constructor
	}
}
