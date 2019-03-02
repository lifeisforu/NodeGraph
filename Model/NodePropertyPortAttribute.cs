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

		public NodePropertyPortAttribute( string displayName, Type type, bool isInput, object defaultValue ) : base( displayName, isInput )
		{
			if( null != defaultValue )
			{
				if( defaultValue.GetType() != type )
				{
					throw new ArgumentException( "Type of value is not same as typeOfvalue." );
				}
			}

			if( !type.IsClass && ( null == defaultValue ) )
			{
				throw new ArgumentException( "If typeOfValue is not a class, you cannot specify value as null" );
			}

			Type = type;
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
