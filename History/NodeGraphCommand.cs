using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeGraph.History
{
	/// <summary>
	/// Command for history stack.
	/// </summary>
	public abstract class NodeGraphCommand
	{
		#region Properties

		/// <summary>
		/// Name of command for display or debugging.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Parameters for executing undo.
		/// </summary>
		public object UndoParams { get; private set; }

		/// <summary>
		/// Parameters for executing redo.
		/// </summary>
		public object RedoParams { get; private set; }

		#endregion // Properties

		#region Methods

		/// <summary>
		/// Cancel command.
		/// </summary>
		public abstract void Undo();

		/// <summary>
		/// Restore command.
		/// </summary>
		public abstract void Redo();

		#endregion // Methods

		#region Constructor

		public NodeGraphCommand( string name, object undoParams, object redoParams )
		{
			Name = name;
			UndoParams = undoParams;
			RedoParams = redoParams;
		}

		#endregion // Constructor
	}
}
