using NodeGraph.ViewModel;
using System;

namespace NodeGraph.Model
{
	[AttributeUsage( AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum )]
	public class NodeAttribute : Attribute
	{
		public Type ViewModelType = typeof( NodeViewModel );

		public NodeAttribute()
		{
			if( !typeof( NodeViewModel ).IsAssignableFrom( ViewModelType ) )
				throw new ArgumentException( "ViewModelType of NodeAttribute must be subclass of NodeViewModel" );
		}
	}
}
