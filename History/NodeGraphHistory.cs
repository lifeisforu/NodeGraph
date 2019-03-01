using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeGraph.History
{
	/// <summary>
	/// History stack for NodeGraph. 
	/// </summary>
	public class NodeGraphHistory
	{
		#region Transaction

		public class Transaction
		{
			public string Name { get; private set; }

			public List<NodeGraphCommand> _Commands = new List<NodeGraphCommand>();

			public Transaction( string name )
			{
				Name = name;
			}

			public void Add( NodeGraphCommand command )
			{
				_Commands.Add( command );
			}

			internal void Undo()
			{
				for( int i = _Commands.Count - 1; i >= 0; --i )
				{
					_Commands[ i ].Undo();
				}
			}

			internal void Redo()
			{
				for( int i = 0; i < _Commands.Count; ++i )
				{
					_Commands[ i ].Redo();
				}
			}
		};

		#endregion // Transaction

		#region Fields

		private List<Transaction> _Transactions = new List<Transaction>();

		private bool _IsProcessingHistory = false;

		private int _CurrentPos = -1;

		#endregion // Fields

		#region Properties

		public object Owner { get; private set; }

		#endregion Properites

		#region Constructor

		public NodeGraphHistory( object owner, int numTransactions = 100 )
		{
			Owner = owner;

			SetNumberOfTransactions( numTransactions );
		}

		#endregion // Constructor

		#region Private Methods

		#endregion // Private Methods

		#region Public Methods

		public void Clear()
		{
			_Transactions.Clear();
			_CurrentPos = -1;
		}

		public int GetNumberOfTransactions()
		{
			return _Transactions.Count;
		}

		public void SetNumberOfTransactions( int numTransactions, bool bClear = false )
		{
			if( bClear )
			{
				_Transactions.Clear();
				_CurrentPos = -1;
			}

			int prevNumTransactions = _Transactions.Count;

			if( prevNumTransactions < numTransactions )
			{
				List<Transaction> newTransactions = new List<Transaction>();
				for( int i = 0; i < numTransactions; ++i )
				{
					if( i < prevNumTransactions )
					{
						newTransactions.Add( _Transactions[ i ] );
					}
					else
					{
						newTransactions.Add( null );
					}
				}
				_Transactions = newTransactions;
			}
			else if( prevNumTransactions > numTransactions )
			{
				List<Transaction> newTransactions = new List<Transaction>();

				int start = 0;
				if( _CurrentPos >= numTransactions )
				{
					start = _CurrentPos - numTransactions;	
				}
				else
				{
					start = prevNumTransactions - 1 - numTransactions;
				}

				int count = 0;
				for( int i = start; i <= _CurrentPos; ++i, ++count )
				{
					newTransactions.Add( _Transactions[ i ] );
				}

				for( int i = 0; i < numTransactions - count; ++i )
				{
					newTransactions.Add( null );
				}

				_Transactions = newTransactions;
			}
		}

		public List<Transaction> GetTransactions()
		{
			return _Transactions;
		}

		public void GetTransactionList( out List<Transaction> undoTransactionList, 
			out Transaction currentTransaction, 
			out List<Transaction> redoTransactionList )
		{
			undoTransactionList = new List<Transaction>();
			currentTransaction = null;
			redoTransactionList = new List<Transaction>();

			if( -1 == _CurrentPos )
			{
				return;
			}

			for( int i = 0; i < _CurrentPos; ++i )
			{
				undoTransactionList.Add( _Transactions[ i ] );
			}

			currentTransaction = _Transactions[ _CurrentPos ];

			for( int i = _CurrentPos + 1; i < _Transactions.Count; ++i )
			{
				redoTransactionList.Add( _Transactions[ i ] );
			}
		}

		private Transaction _TransactionAdding = null;
		public void BeginTransaction( string name )
		{
			if( _IsProcessingHistory )
			{
				return;
			}

			if( null != _TransactionAdding )
			{
				EndTransaction( true );
			}

			_TransactionAdding = new Transaction( name );

			if( NodeGraphManager.OutputDebugInfo )
			{
				System.Diagnostics.Debug.WriteLine( string.Format( "BeginTransaction {0}", _TransactionAdding.Name ) );
			}
		}
		
		public void AddCommand( NodeGraphCommand command )
		{
			if( _IsProcessingHistory )
			{
				return;
			}

			if( null != _TransactionAdding )
			{
				_TransactionAdding.Add( command );

				if( NodeGraphManager.OutputDebugInfo )
				{
					System.Diagnostics.Debug.WriteLine( string.Format( "Add command to transaction {0} : {1}", _TransactionAdding.Name, command.Name ) );
				}
			}
		}

		public void EndTransaction( bool bCancel )
		{
			if( _IsProcessingHistory )
			{
				return;
			}

			if( null == _TransactionAdding )
			{
				return;
			}

			if( !bCancel )
			{
				int nextPos = _CurrentPos + 1;
				if( nextPos >= _Transactions.Count ) // shift
				{
					_Transactions.RemoveAt( 0 );
					_Transactions.Add( null );
				}
				else
				{
					_CurrentPos = nextPos;
				}

				_Transactions[ _CurrentPos ] = _TransactionAdding;

				for( int i = _CurrentPos + 1; i < _Transactions.Count; ++i )
				{
					_Transactions[ i ] = null;
				}

			}

			if( NodeGraphManager.OutputDebugInfo )
			{
				if( bCancel )
					System.Diagnostics.Debug.WriteLine( string.Format( "CancelTransaction {0}", _TransactionAdding.Name ) );
				else
					System.Diagnostics.Debug.WriteLine( string.Format( "EndTransaction {0}", _TransactionAdding.Name ) );
			}

			_TransactionAdding = null;
		}

		public void Undo()
		{
			MoveTo( _CurrentPos - 1 );
		}

		public void Redo()
		{
			MoveTo( _CurrentPos + 1 );
		}

		public void MoveTo( int pos )
		{
			_IsProcessingHistory = true;

			int realPos = Math.Min( _Transactions.Count - 1, Math.Max( -1, pos ) );

			// redo.
			if( _CurrentPos < realPos )
			{
				int lastPos = -1000;
				for( int i = _CurrentPos + 1; i <= realPos; ++i )
				{
					if( null == _Transactions[ i ] )
						break;

					_Transactions[ i ].Redo();
					lastPos = i;

					if( NodeGraphManager.OutputDebugInfo )
					{
						System.Diagnostics.Debug.WriteLine( string.Format( "Redo {0} : {1}", i, _Transactions[ i ].Name ) );
					}
				}

				_CurrentPos = ( -1000 != lastPos ) ? lastPos : _CurrentPos;
			}
			// undo
			else if( _CurrentPos > realPos )
			{
				for( int i = _CurrentPos; i > realPos; --i )
				{
					_Transactions[ i ].Undo();

					if( NodeGraphManager.OutputDebugInfo )
					{
						System.Diagnostics.Debug.WriteLine( string.Format( "Undo {0} : {1}", i, _Transactions[ i ].Name ) );
					}
				}
				_CurrentPos = realPos;
			}
			// nothing.
			else
			{

			}

			_IsProcessingHistory = false;
		}

		#endregion // Public Methods
	}
}
