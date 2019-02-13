using System.ComponentModel;
using System.Windows;

namespace NodeGraph.ViewModel
{
	public class ViewModelBase : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected void RaisePropertyChanged( string propertyName )
		{
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
		}
	}
}
