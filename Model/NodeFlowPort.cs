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
		public NodeFlowPort( Guid guid, Node node, bool isInput ) :
			base( guid, node, isInput )
		{
		}

		#endregion // Constructor

		#region Overrides Callbacks

		public override void OnDeserialize()
		{
			base.OnDeserialize();
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
