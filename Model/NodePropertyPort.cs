using NodeGraph.ViewModel;
using System;
using System.Xml;
using System.Xml.Serialization;

namespace NodeGraph.Model
{
	public class NodePropertyPort : NodePort
	{
		#region Events

		public delegate void ValueChangedDelegate( NodePropertyPort port, object prevValue, object newValue );
		public event ValueChangedDelegate ValueChanged;

		protected virtual void OnValueChanged( object prevValue, object newValue )
		{
			ValueChanged?.Invoke( this, prevValue, newValue );
		}

		#endregion // Events

		#region Properties

		private object _Value;
		public object Value
		{
			get { return _Value; }
			set
			{
				if( value != _Value )
				{
					object prevValue = _Value;
					_Value = value;

					RaisePropertyChanged( "Value" );
					OnValueChanged( prevValue, value );
				}
			}
		}

		public Type TypeOfValue { get; private set; }

		#endregion // Properties

		#region Constructors

		/// <summary>
		/// Never call this constructor directly. Use GraphManager.CreateNodePropertyPort() method.
		/// </summary>
		public NodePropertyPort( Guid guid, Node node, bool isInput, Type typeOfValue, object value ) : 
			base( guid, node, isInput )
		{
			if( null != value )
			{
				if( value.GetType() != typeOfValue )
				{
					throw new ArgumentException( "Type of value is not same as typeOfvalue." );
				}
				Value = value;
			}

			if( !typeOfValue.IsClass && ( null == value ) )
			{
				throw new ArgumentNullException( "If typeOfValue is not a class, you cannot specify value as null" );
			}

			TypeOfValue = typeOfValue;
		}

		#endregion // Constructors

		#region Overrides Callbacks

		public override void OnDeserialize()
		{
			base.OnDeserialize();
		}

		#endregion // Overrides Callbacks

		#region Destructor

		~NodePropertyPort()
		{
		}

		#endregion // Destructor

		#region Overrides IXmlSerializable

		public override void WriteXml( XmlWriter writer )
		{
			base.WriteXml( writer );

			writer.WriteAttributeString( "TypeOfValue", TypeOfValue.AssemblyQualifiedName );
			if( null != Value )
			{
				var serializer = new XmlSerializer( TypeOfValue );
				serializer.Serialize( writer, Value );
			}
		}

		public override void ReadXml( XmlReader reader )
		{
			base.ReadXml( reader );

			TypeOfValue = Type.GetType( reader.GetAttribute( "TypeOfValue" ) );

			if( !reader.IsEmptyElement )
			{
				while( reader.Read() )
				{
					if( XmlNodeType.Element == reader.NodeType )
					{
						var serializer = new XmlSerializer( TypeOfValue );
						Value = serializer.Deserialize( reader );
						break;
					}
				}
			}
		}

		#endregion // Overrides IXmlSerializable
	}
}
