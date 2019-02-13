using NodeGraph.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeGraph.Model
{
	public class ModelBase : INotifyPropertyChanged
	{
		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		protected void RaisePropertyChanged( string propertyName )
		{
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
		}

		#endregion // INotifyPropertyChanged

		#region Properties

		public readonly Guid Guid;

		#endregion // Properties

		#region Constructor

		public ModelBase( Guid guid )
		{
			Guid = guid;
		}

		#endregion // Constructor
	}
}
