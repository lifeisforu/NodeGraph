using NodeGraph.ViewModel;
using System;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace NodeGraph.Model
{
	public class NodePropertyPort : NodePort
	{
		#region Events

		public delegate void DynamicPropertyPortValueChangedDelegate( NodePropertyPort port, object prevValue, object newValue );
		public event DynamicPropertyPortValueChangedDelegate DynamicPropertyPortValueChanged;

		protected virtual void OnDynamicPropertyPortValueChanged( object prevValue, object newValue )
		{
			DynamicPropertyPortValueChanged?.Invoke( this, prevValue, newValue );
		}

		#endregion // Events

		#region Fields

		public readonly bool IsDynamic;
		public readonly bool HasEditor;
		protected FieldInfo _FieldInfo;
		protected PropertyInfo _PropertyInfo;

		#endregion // Fields

		#region Properties

		public object _Value;
		public object Value
		{
			get
			{
				if( IsDynamic )
				{
					return _Value;
				}
				else
				{
					return ( null != _FieldInfo ) ? _FieldInfo.GetValue( Owner ) : _PropertyInfo.GetValue( Owner );
				}
			}
			set
			{
				object prevValue;
				if( IsDynamic )
				{
					prevValue = _Value;
				}
				else
				{
					prevValue = ( null != _FieldInfo ) ? _FieldInfo.GetValue( Owner ) : _PropertyInfo.GetValue( Owner );
				}
				
				if( value != prevValue )
				{
					if( IsDynamic )
					{
						_Value = value;
						OnDynamicPropertyPortValueChanged( prevValue, value );
					}
					else
					{
						if( null != _FieldInfo )
							_FieldInfo.SetValue( Owner, value );
						else if( null != _PropertyInfo )
							_PropertyInfo.SetValue( Owner, value );
					}

					RaisePropertyChanged( "Value" );
				}
			}
		}
		
		public Type ValueType { get; private set; }

		#endregion // Properties

		#region Constructors

		/// <summary>
		/// Never call this constructor directly. Use GraphManager.CreateNodePropertyPort() method.
		/// </summary>
		public NodePropertyPort( Guid guid, Node node, bool isInput, Type valueType, object value, string name, bool hasEditor ) : 
			base( guid, node, isInput )
		{
			Name = name;
			HasEditor = hasEditor;

			Type nodeType = node.GetType();
			_FieldInfo = nodeType.GetField( Name );
			_PropertyInfo = nodeType.GetProperty( Name );
			IsDynamic = ( null == _FieldInfo ) && ( null == _PropertyInfo );

			ValueType = valueType;
			Value = value;
		}

		#endregion // Constructors

		#region Overrides IXmlSerializable

		public override void WriteXml( XmlWriter writer )
		{
			base.WriteXml( writer );

			writer.WriteAttributeString( "ValueType", ValueType.AssemblyQualifiedName );
			writer.WriteAttributeString( "HasEditor", HasEditor.ToString() );

			Type realValueType = ValueType;
			if( null != Value )
			{
				realValueType = Value.GetType();
			}
			writer.WriteAttributeString( "RealValueType", realValueType.AssemblyQualifiedName );

			var serializer = new XmlSerializer( realValueType );
			serializer.Serialize( writer, Value );
		}

		public override void ReadXml( XmlReader reader )
		{
			base.ReadXml( reader );

			Type realValueType = Type.GetType( reader.GetAttribute( "RealValueType" ) );
			
			while( reader.Read() )
			{
				if( XmlNodeType.Element == reader.NodeType )
				{
					var serializer = new XmlSerializer( realValueType );
					Value = serializer.Deserialize( reader );
					break;
				}
			}
		}

		#endregion // Overrides IXmlSerializable

		#region Callbacks

		public override void OnCreate()
		{
			base.OnCreate();

			CheckValidity();
		}

		public override void OnDeserialize()
		{
			base.OnDeserialize();

			CheckValidity();
		}

		#endregion // Callbacks

		#region Methods

		public void CheckValidity()
		{
			if( null != Value )
			{
				if( Value.GetType() != ValueType )
				{
					throw new ArgumentException( "Type of value is not same as typeOfvalue." );
				}
			}

			if( !ValueType.IsClass && ( null == Value ) )
			{
				throw new ArgumentNullException( "If typeOfValue is not a class, you cannot specify value as null" );
			}

			if( !IsDynamic )
			{
				Type nodeType = Owner.GetType();
				_FieldInfo = nodeType.GetField( Name );
				_PropertyInfo = nodeType.GetProperty( Name );

				Type propType = ( null != _FieldInfo ) ? _FieldInfo.GetValue( Owner ).GetType() : _PropertyInfo.GetValue( Owner ).GetType();
				if( propType != ValueType )
				{
					throw new ArgumentException( string.Format( "ValueType( {0} ) is invalid, becasue a type of property or field is {1}.",
						ValueType.Name, propType.Name ) );
				}
			}
		}

		#endregion // Methods
	}
}
