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
	public class NodePropertyPortView : NodePortView
	{
		#region Template

		static NodePropertyPortView()
		{
			DefaultStyleKeyProperty.OverrideMetadata( typeof( NodePropertyPortView ), new FrameworkPropertyMetadata( typeof( NodePropertyPortView ) ) );
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
		}

		#endregion // Template

		#region Constructor

		public NodePropertyPortView( bool isInput ) : base( isInput )
		{
		}

		#endregion // Constructor

		#region ViewModels

		public NodePropertyPortViewModel NodePropertyPortViewModel
		{
			get { return DataContext as NodePropertyPortViewModel; }
		}

		#endregion // ViewModels
	}
}
