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

		private ObservableCollection<ConnectorViewModel> _ConnectorViewModels = new ObservableCollection<ConnectorViewModel>();
		public ObservableCollection<ConnectorViewModel> ConnectorViewModels
		{
			get { return _ConnectorViewModels; }
			set
			{
				if( value != _ConnectorViewModels )
				{
					_ConnectorViewModels = value;
					RaisePropertyChanged( "ConnectorViewModels" );
				}
			}
		}

		#endregion // Properties

		#region Constructor

		public NodePortViewModel( NodePort nodePort )
		{
			Model = nodePort ?? throw new ArgumentNullException( "NodePort can not be null in NodePortViewModel constructor." );
		}

		#endregion // Constructor
	}
}
