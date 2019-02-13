using NodeGraph.ViewModel;
using System;

namespace NodeGraph.Model
{
	[AttributeUsage( AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum )]
	public class NodeAttribute : Attribute
	{
		public Type ViewModelType = typeof( NodeViewModel );
		public string Header;
		public string HeaderBackgroundColor = "Black";
		public string HeaderFontColor = "White";
		public bool AllowCircularConnection = false;

		public NodeAttribute( string header )
		{
			Header = header;
			if( !typeof( NodeViewModel ).IsAssignableFrom( ViewModelType ) )
				throw new ArgumentException( "ViewModelType of NodeAttribute must be subclass of NodeViewModel" );
		}
	}
}
