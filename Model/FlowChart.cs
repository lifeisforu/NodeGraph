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

		#region Callbacks

		public virtual void OnCreate()
		{
			if( NodeGraphManager.OutputDebugInfo )
				System.Diagnostics.Debug.WriteLine( "FlowChart.OnCreate()" );
		}

		public virtual void OnPreExecute()
		{
			if( NodeGraphManager.OutputDebugInfo )
				System.Diagnostics.Debug.WriteLine( "FlowChart.OnPreExecute()" );
		}

		public virtual void OnExecute()
		{
			if( NodeGraphManager.OutputDebugInfo )
				System.Diagnostics.Debug.WriteLine( "FlowChart.OnExecute()" );
		}

		public virtual void OnPostExecute()
		{
			if( NodeGraphManager.OutputDebugInfo )
				System.Diagnostics.Debug.WriteLine( "FlowChart.OnPostExecute()" );
		}

		public virtual void OnPreDestroy()
		{
			if( NodeGraphManager.OutputDebugInfo )
				System.Diagnostics.Debug.WriteLine( "FlowChart.OnPreDestroy()" );
		}

		public virtual void OnPostDestroy()
		{
			if( NodeGraphManager.OutputDebugInfo )
				System.Diagnostics.Debug.WriteLine( "FlowChart.OnPostDestroy()" );
		}

		#endregion // Callbacks
	}
}
