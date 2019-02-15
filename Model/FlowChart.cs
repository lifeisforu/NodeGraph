using NodeGraph.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeGraph.Model
{
	[FlowChart()]
	public class FlowChart : ModelBase
	{
		#region Properties

		protected FlowChartViewModel _ViewModel;
		public FlowChartViewModel ViewModel
		{
			get { return _ViewModel; }
			set
			{
				if( value != _ViewModel )
				{
					_ViewModel = value;
					RaisePropertyChanged( "ViewModel" );
				}
			}
		}

		protected ObservableCollection<Node> _Nodes = new ObservableCollection<Node>();
		public ObservableCollection<Node> Nodes
		{
			get { return _Nodes; }
			set
			{
				if( value != _Nodes )
				{
					_Nodes = value;
					RaisePropertyChanged( "Nodes" );
				}
			}
		}

		protected ObservableCollection<Connector> _Connectors = new ObservableCollection<Connector>();
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

		/// <summary>
		/// Never call this constructor directly. Use GraphManager.CreateFlowChart() method.
		/// </summary>
		public FlowChart( Guid guid ) : base( guid )
		{
			
		}

		#endregion // Constructor

		#region Create Events

		public event EventHandler Create;

		public void InvokeCreateEvent()
		{
			EventArgs args = new EventArgs();

			OnCreate();

			Create?.Invoke( this, new EventArgs() );
		}

		protected virtual void OnCreate()
		{

		}

		#endregion // Create Events
	}
}
