using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace NodeGraph.ViewModel
{
	public class ViewModelBase : INotifyPropertyChanged
	{
		#region Overrides InotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		protected void RaisePropertyChanged( string propertyName )
		{
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
		}

		#endregion // Overrides INotifyPropertyChanged
	}
}
