using NodeGraph.Model;
using NodeGraph.View;
using System.Windows.Media;

namespace NodeGraph.History
{
	public class ZoomAndPanCommand : NodeGraphCommand
	{
		#region Additional Parameters

		public FlowChart FlowChart { get; private set; }

		#endregion // Additional Parameters

		#region Constructor

		public ZoomAndPanCommand( string name, FlowChart flowChart, object undoParams, object redoParams ) : base( name, undoParams, redoParams )
		{
			FlowChart = flowChart;
		}

		#endregion // Constructor

		#region Overrides NodeGraphCommand

		public override void Undo()
		{
			Matrix matrix = ( Matrix )UndoParams;

			FlowChartView view = FlowChart.ViewModel.View;

			view.ZoomAndPan.StartX = -matrix.OffsetX;
			view.ZoomAndPan.StartY = -matrix.OffsetY;
			view.ZoomAndPan.Scale = matrix.M11;
		}

		public override void Redo()
		{
			Matrix matrix = ( Matrix )RedoParams;

			FlowChartView view = FlowChart.ViewModel.View;

			view.ZoomAndPan.StartX = -matrix.OffsetX;
			view.ZoomAndPan.StartY = -matrix.OffsetY;
			view.ZoomAndPan.Scale = matrix.M11;
		}

		#endregion // Overrides NodeGraphCommand
	}
}
