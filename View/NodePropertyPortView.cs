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
	[TemplatePart( Name = "PART_PortTextBlock", Type = typeof( FrameworkElement ) )]
	public class NodePropertyPortView : NodePortView
	{
		#region Properteis

		public FrameworkElement PartPortTextBlock { get; private set; }

		#endregion // Properites

		#region Template

		static NodePropertyPortView()
		{
			DefaultStyleKeyProperty.OverrideMetadata( typeof( NodePropertyPortView ), new FrameworkPropertyMetadata( typeof( NodePropertyPortView ) ) );
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			PartPortTextBlock = Template.FindName( "PART_PortTextBlock", this ) as FrameworkElement;
			if( null == PartPortTextBlock )
				throw new Exception( "PartPortTextBlock can not be null in NodePropertyPortView" );
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
