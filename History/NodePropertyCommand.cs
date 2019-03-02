using NodeGraph.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NodeGraph.History
{
	public class NodePropertyCommand : NodeGraphCommand
	{
		#region Additional information

		public Guid Guid { get; private set; }
		public string PropertyName { get; private set; }

		#endregion // additional information

		#region Constructor

		public NodePropertyCommand( string name, Guid nodeGuid, string propertyName, object undoParams, object redoParams ) : base( name, undoParams, redoParams )
		{
			Guid = nodeGuid;
			PropertyName = propertyName;
		}

		#endregion // Constructor

		#region Overrides NodeGraphCommand

		public override void Undo()
		{
			Node node = NodeGraphManager.FindNode( Guid );
			if( null == node )
			{
				throw new InvalidOperationException( "Node does not exist." );
			}

			if( "IsSelected" == PropertyName )
			{
				UpdateSelection( ( bool )UndoParams );
			}
			else
			{
				Type type = node.GetType();
				PropertyInfo propInfo = type.GetProperty( PropertyName );
				propInfo.SetValue( node, UndoParams );
			}
		}

		public override void Redo()
		{
			Node node = NodeGraphManager.FindNode( Guid );
			if( null == node )
			{
				throw new InvalidOperationException( "Node does not exist." );
			}

			if( "IsSelected" == PropertyName )
			{
				UpdateSelection( ( bool )RedoParams );
			}
			else
			{
				Type type = node.GetType();
				PropertyInfo propInfo = type.GetProperty( PropertyName );
				propInfo.SetValue( node, RedoParams );
			}
		}

		#endregion // Overrides NodeGraphCommand

		#region Private Methods

		private void UpdateSelection( bool isSelected )
		{
			Node node = NodeGraphManager.FindNode( Guid );

			ObservableCollection<Guid> selectionList = NodeGraphManager.GetSelectionList( node.Owner );

			node.ViewModel.IsSelected = isSelected;

			if( node.ViewModel.IsSelected )
			{
				System.Diagnostics.Debug.WriteLine( "True" );
				if( !selectionList.Contains( Guid ) )
				{
					selectionList.Add( Guid );
				}
			}
			else
			{
				System.Diagnostics.Debug.WriteLine( "False" );
				if( selectionList.Contains( Guid ) )
				{
					selectionList.Remove( Guid );
				}
			}
		}

		#endregion // Private Methods
	}
}
