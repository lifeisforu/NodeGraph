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

		public NodePropertyPort( Guid guid, Node node, NodePropertyPortAttribute attr ) : base( guid, node, attr )
		{
			if( null != attr.DefaultValue )
				Value = attr.DefaultValue;

			TypeOfValue = attr.Type;
		}

		#endregion // Constructors

		#region Destructor

		~NodePropertyPort()
		{
		}

		#endregion // Destructor
	}
}
