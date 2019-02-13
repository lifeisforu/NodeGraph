using NodeGraph.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeGraph.Model
{
	[AttributeUsage( AttributeTargets.Class )]
	public class ConnectorAttribute : Attribute
	{
		public Type ViewModelType = typeof( ConnectorViewModel );

		public ConnectorAttribute()
		{
			if( !typeof( ConnectorViewModel ).IsAssignableFrom( ViewModelType ) )
				throw new ArgumentException( "ViewModelType of ConnectorAttribute must be subclass of ConnectorViewModel" );
		}
	}
}
