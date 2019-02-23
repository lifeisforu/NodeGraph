using NodeGraph.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace NodeGraph.Model
{
	public class ModelBase : INotifyPropertyChanged, IXmlSerializable
	{
		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		protected void RaisePropertyChanged( string propertyName )
		{
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
		}

		#endregion // INotifyPropertyChanged

		#region IXmlSerializable

		public XmlSchema GetSchema()
		{
			return null;
		}

		public virtual void WriteXml( XmlWriter writer )
		{
			writer.WriteAttributeString( "Guid", Guid.ToString() );
		}

		public virtual void ReadXml( XmlReader reader )
		{
			string guidString = reader.GetAttribute( "Guid" );
			if( string.IsNullOrEmpty( guidString ) )
				throw new Exception( "Guid attribute must exist." );
			Guid = Guid.Parse( guidString );
		}

		#endregion IXmlSerializable

		#region Properties

		public Guid Guid { get; private set; }
		public bool IsDeserializedFromXml = false;

		#endregion // Properties

		#region Constructor

		public ModelBase()
		{
			IsDeserializedFromXml = true;
		}

		public ModelBase( Guid guid )
		{
			Guid = guid;
		}

		#endregion // Constructor
	}
}
