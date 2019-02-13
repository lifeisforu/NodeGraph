using NodeGraph.ViewModel;
using System;
using System.Collections.ObjectModel;

namespace NodeGraph.Model
{
	public class NodePort : ModelBase
	{
		#region Fields

		public NodePortViewModel ViewModel;

		public readonly Node Owner;

		public readonly string Name;

		public readonly bool IsInput;

		public readonly bool AllowMultipleInput;

		#endregion // Fields

		#region Properties

		private string _DisplayName;
		public string DisplayName
		{
			get { return _DisplayName; }
			set
			{
				if( value != _DisplayName )
				{
					_DisplayName = value;
					RaisePropertyChanged( "DisplayName" );
				}
			}
		}

		private ObservableCollection<Connector> _Connectors = new ObservableCollection<Connector>();
		public ObservableCollection<Connector> Connectors
		{
			get { return _Connectors; }
			set
			{
				if( value != _Connectors )
				{
					_Connectors = value;
					RaisePropertyChanged( "Connectors" );
				}
			}
		}

		#endregion // Properties

		#region Constructor

		public NodePort( Guid guid, Node node, NodePortAttribute attr ) : base( guid )
		{
			Owner = node;
			Name = attr.Name;
			DisplayName = attr.DisplayName;
			IsInput = attr.IsInput;
			AllowMultipleInput = attr.AllowMultipleInput;
		}

		#endregion // Constructor

		#region Destructor

		~NodePort()
		{
		}

		#endregion // Destructor
	}
}
