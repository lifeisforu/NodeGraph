using NodeGraph.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeGraph.History
{
	public class CreateNodePortCommand : NodeGraphCommand
	{
		#region Additional Parameters

		public Guid NodeGuid { get; private set; }

		public Guid PortGuid { get; private set; }

		#endregion // Additional Parameters

		#region Constructor

		public CreateNodePortCommand( string name, object undoParams, object redoParams ) : base( name, undoParams, redoParams )
		{
		}

		#endregion // Constructor

		#region Overrides NodeGraphCommand

		public override void Undo()
		{
			Guid guid = ( Guid )UndoParams;

			NodeGraphManager.DestroyNodePort( guid );
		}

		public override void Redo()
		{
			NodeGraphManager.DeserializeNodePort( RedoParams as string );
		}

		#endregion // Overrides NodeGraphCommand
	}
}
