using NodeGraph.Model;
using NodeGraph.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace NodeGraph.ViewModel
{
	public class NodePortViewModel : ViewModelBase
	{
		#region Fields

		public NodePortView View;

		#endregion // Fields

		#region Properties

		private NodePort _Model;
		public NodePort Model
		{
			get { return _Model; }
			set
			{
				if( value != _Model )
				{
					_Model = value;
					RaisePropertyChanged( "Model" );
				}
			}
		}

		#endregion // Properties

		#region Constructor

		public NodePortViewModel( NodePort nodePort )
		{
			Model = nodePort ?? throw new ArgumentNullException( "NodePort can not be null in NodePortViewModel constructor." );
			Model.Connectors.CollectionChanged += _ConnectorViewModels_CollectionChanged;
		}

		#endregion // Constructor

		#region Collection Events

		private void _ConnectorViewModels_CollectionChanged( object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e )
		{
			Node node = Model.Owner;

			if( null != e.OldItems )
			{
				foreach( var item in e.OldItems )
				{
					var removedConnector = item as Connector;
					node.ViewModel.OnConnectionRemoved( this );
				}
			}

			if( null != e.NewItems )
			{
				foreach( var item in e.NewItems )
				{
					var addedConnector = item as Connector;
					node.ViewModel.OnConnectionAdded( this );
				}
			}

			if( null != View )
			{
				View.OnConnectionChanged();
			}
		}

		#endregion // Collection Events

		#region ContextMenu

		public delegate void BuildContextMenuEventHandler( object sender, BuildContextMenuEventArgs e );

		public static event BuildContextMenuEventHandler BuildContextMenu;

		public static bool ContextMenuEnabled
		{
			get { return ( null != BuildContextMenu ) && ( 0 < BuildContextMenu.GetInvocationList().Length ); }
		}

		public void InvokeBuildContextMenuEvent( BuildContextMenuEventArgs e )
		{
			BuildContextMenu?.Invoke( this, e );
		}

		#endregion // ContextMenu
	}
}
