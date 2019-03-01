using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeGraph.History
{
	public class DestroyConnectorCommand : NodeGraphCommand
	{
		#region Constructor

		public DestroyConnectorCommand( string name, object undoParams, object redoParams ) : base( name, undoParams, redoParams )
		{

		}

		#endregion // Constructor

		#region Overrides NodeGraphCommand

		public override void Undo()
		{
			NodeGraphManager.DeserializeConnector( UndoParams as string );
		}

		public override void Redo()
		{
			Guid guid = ( Guid )RedoParams;

			NodeGraphManager.DestroyConnector( guid );
		}

		#endregion // Overrides NodeGraphCommand
	}
}
