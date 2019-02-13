using NodeGraph.Model;
using NodeGraph.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NodeGraph.View
{
	public class NodeFlowPortView : NodePortView
	{
		#region Template

		static NodeFlowPortView()
		{
			DefaultStyleKeyProperty.OverrideMetadata( typeof( NodeFlowPortView ), new FrameworkPropertyMetadata( typeof( NodeFlowPortView ) ) );
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
		}

		#endregion // Template

		#region Constructor

		public NodeFlowPortView( bool isInput ) : base( isInput )
		{
			
		}

		#endregion // Constructor

		#region ViewModels

		public NodeFlowPortViewModel NodeFlowPortViewModel
		{
			get { return DataContext as NodeFlowPortViewModel; }
		}

		#endregion // ViewModels
	}
}
