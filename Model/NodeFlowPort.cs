using NodeGraph.ViewModel;
using System;
using System.Xml;

namespace NodeGraph.Model
{
	public class NodeFlowPort : NodePort
	{
		#region Properties

		#endregion // Properties

		#region Constructor

		/// <summary>
		/// Never call this constructor directly. Use GraphManager.CreateNodeFlowPort() method.
		/// </summary>
		public NodeFlowPort( Guid guid, Node node, string name, string displayName, bool isInput, bool allowMultipleInput, bool allowMultipleOutput ) :
			base( guid, node, name, displayName, isInput, allowMultipleInput, allowMultipleOutput )
		{
		}

		#endregion // Constructor

		#region Overrides Callbacks

		public override void OnPostLoad()
		{

		}

		#endregion // Overrides Callbacks

		#region Overrides IXmlSerializable

		public override void WriteXml( XmlWriter writer )
		{
			base.WriteXml( writer );
		}

		public override void ReadXml( XmlReader reader )
		{
			base.ReadXml( reader );
		}

		#endregion // Overrides IXmlSerializable
	}
}
