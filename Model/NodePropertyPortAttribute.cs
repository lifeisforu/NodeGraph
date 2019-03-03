using NodeGraph.ViewModel;
using System;
using System.Windows.Media;

namespace NodeGraph.Model
{
	[AttributeUsage( AttributeTargets.Field | AttributeTargets.Property )]
	public class NodePropertyPortAttribute : NodePortAttribute
	{
		public Type ValueType;
		public Type ViewModelType = typeof( NodePropertyPortViewModel );
		public object DefaultValue;
		public bool HasEditor;

		public NodePropertyPortAttribute( string displayName, bool isInput, Type valueType, object defaultValue, bool hasEditor ) : base( displayName, isInput )
		{
			if( null != defaultValue )
			{
				if( ( valueType == typeof( Color ) ) && ( defaultValue.GetType() == typeof( string ) ) )
				{
					defaultValue = ColorConverter.ConvertFromString( defaultValue as string );
				}
				else if( defaultValue.GetType() != valueType )
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
			HasEditor = hasEditor;

			if( !typeof( NodePropertyPortViewModel ).IsAssignableFrom( ViewModelType ) )
			{
				throw new ArgumentException( "ViewModelType of NodePropertyAttribute must be subclass of NodePropertyPortViewModel" );
			}
		}
	}
}
