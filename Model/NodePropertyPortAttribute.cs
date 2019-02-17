using NodeGraph.ViewModel;
using System;

namespace NodeGraph.Model
{
	[AttributeUsage( AttributeTargets.Field | AttributeTargets.Property )]
	public class NodePropertyPortAttribute : NodePortAttribute
	{
		public Type Type;
		public Type ViewModelType = typeof( NodePropertyPortViewModel );
		public object DefaultValue;

		public NodePropertyPortAttribute( string displayName, Type type, bool isInput ) : base( displayName, isInput )
		{
			Type = type;
			IsInput = isInput;
			AllowMultipleInput = false;
			AllowMultipleOutput = true;

			if( !typeof( NodePropertyPortViewModel ).IsAssignableFrom( ViewModelType ) )
				throw new ArgumentException( "ViewModelType of NodePropertyAttribute must be subclass of NodePropertyPortViewModel" );
		}
	}
}
