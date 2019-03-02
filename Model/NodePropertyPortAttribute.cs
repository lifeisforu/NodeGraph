using NodeGraph.ViewModel;
using System;

namespace NodeGraph.Model
{
	[AttributeUsage( AttributeTargets.Field | AttributeTargets.Property )]
	public class NodePropertyPortAttribute : NodePortAttribute
	{
		public Type ValueType;
		public Type ViewModelType = typeof( NodePropertyPortViewModel );
		public object DefaultValue;

		public NodePropertyPortAttribute( string displayName, Type valueType, bool isInput, object defaultValue ) : base( displayName, isInput )
		{
			if( null != defaultValue )
			{
				if( defaultValue.GetType() != valueType )
				{
					throw new ArgumentException( "Type of value is not same as ValueType." );
				}
			}

			if( !valueType.IsClass && ( null == defaultValue ) )
			{
				throw new ArgumentException( "If ValueType is not a class, you cannot specify value as null" );
			}

			ValueType = valueType;
			IsInput = isInput;
			DefaultValue = defaultValue;
			AllowMultipleInput = false;
			AllowMultipleOutput = true;

			if( !typeof( NodePropertyPortViewModel ).IsAssignableFrom( ViewModelType ) )
			{
				throw new ArgumentException( "ViewModelType of NodePropertyAttribute must be subclass of NodePropertyPortViewModel" );
			}
		}
	}
}
