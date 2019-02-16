using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace NodeGraph.ViewModel
{
	public class BuildContextMenuEventArgs : EventArgs
	{
		public ContextMenu ContextMenu;
		public Point MouseLocation;

		public BuildContextMenuEventArgs( ContextMenu contextMenu, Point location )
		{
			ContextMenu = contextMenu;
			MouseLocation = location;
		}
	}

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
