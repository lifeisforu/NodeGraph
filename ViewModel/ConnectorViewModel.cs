using NodeGraph.Model;
using NodeGraph.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeGraph.ViewModel
{
	public class ConnectorViewModel : ViewModelBase
	{
		#region Fields

		public ConnectorView View;

		#endregion // Fields

		#region Properties

		private Connector _Model;
		public Connector Model
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

		public ConnectorViewModel( Connector connection )
		{
			Model = connection;
		}

		#endregion // Constructor
	}
}
