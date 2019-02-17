using NodeGraph.ViewModel;
using System;

namespace NodeGraph.Model
{
	public class NodePropertyPort : NodePort
	{
		#region Properties

		private object _Value;
		public object Value
		{
			get { return _Value; }
			set
			{
				if( value != _Value )
				{
					_Value = value;
					RaisePropertyChanged( "Value" );
				}
			}
		}

		public Type TypeOfValue { get; private set; }

		#endregion // Properties

		#region Constructors

		/// <summary>
		/// Never call this constructor directly. Use GraphManager.CreateNodePropertyPort() method.
		/// </summary>
		public NodePropertyPort( Guid guid, Node node, string name, string displayName, bool isInput, bool allowMultipleInput, bool allowMltipleOutput, Type typeOfValue, object value ) : 
			base( guid, node, name, displayName, isInput, allowMultipleInput, allowMltipleOutput )
		{
			if( null != value )
				Value = value;

			if( !typeOfValue.IsClass && ( null == value ) )
				throw new ArgumentNullException( "If typeOfValue is not a class, you cannot specify value as null" );

			TypeOfValue = typeOfValue;
		}

		#endregion // Constructors

		#region Destructor

		~NodePropertyPort()
		{
		}

		#endregion // Destructor
	}
}
