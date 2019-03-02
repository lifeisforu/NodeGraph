using NodeGraph.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.Xml;

namespace NodeGraph.Model
{
	public class NodePort : ModelBase
	{
		#region Fields

		public readonly bool IsInput;

		public readonly Node Owner;

		#endregion // Fields

		#region Properties

		public NodePortViewModel ViewModel { get; set; }

		private string _Name;
		public string Name
		{
			get { return _Name; }
			set
			{
				if( value != _Name )
				{
					_Name = value;
					RaisePropertyChanged( "Name" );
				}
			}
		}

		private bool _AllowMultipleInput;
		public bool AllowMultipleInput
		{
			get { return _AllowMultipleInput; }
			set
			{
				if( value != _AllowMultipleInput )
				{
					_AllowMultipleInput = value;
					RaisePropertyChanged( "AllowMultipleInput" );
				}
			}
		}

		private bool _AllowMultipleOutput;
		public bool AllowMultipleOutput
		{
			get { return _AllowMultipleOutput; }
			set
			{
				if( value != _AllowMultipleOutput )
				{
					_AllowMultipleOutput = value;
					RaisePropertyChanged( "AllowMultipleOutput" );
				}
			}
		}

		private string _DisplayName;
		public string DisplayName
		{
			get { return _DisplayName; }
			set
			{
				if( value != _DisplayName )
				{
					_DisplayName = value;
					RaisePropertyChanged( "DisplayName" );
				}
			}
		}

		private ObservableCollection<Connector> _Connectors = new ObservableCollection<Connector>();
		public ObservableCollection<Connector> Connectors
		{
			get { return _Connectors; }
			set
			{
				if( value != _Connectors )
				{
					_Connectors = value;
					RaisePropertyChanged( "Connectors" );
				}
			}
		}

		private bool _IsPortEnabled = true;
		public bool IsPortEnabled
		{
			get { return _IsPortEnabled; }
			set
			{
				if( value != _IsPortEnabled )
				{
					_IsPortEnabled = value;
					RaisePropertyChanged( "IsPortEnabled" );
				}
			}
		}

		private bool _IsEnabled = true;
		public bool IsEnabled
		{
			get { return _IsEnabled; }
			set
			{
				if( value != _IsEnabled )
				{
					_IsEnabled = value;
					RaisePropertyChanged( "IsEnabled" );
				}
			}
		}
		
		#endregion // Properties

		#region Constructor

		/// <summary>
		/// Never call this constructor directly. Use GraphManager.CreateNodeFlowPort() or GraphManager.CreateNodePropertyPort() method.
		/// </summary>
		protected NodePort( Guid guid, Node node, bool isInput ) : base( guid )
		{
			Owner = node;
			IsInput = isInput;
		}

		#endregion // Constructor

		#region Destructor

		~NodePort()
		{
		}

		#endregion // Destructor
				
		#region Methods

		public virtual bool IsConnectable( NodePort otherPort, out string error )
		{
			error = "";
			return true;
		}

		#endregion // Methods

		#region Callbacks

		public virtual void OnCreate()
		{
			if( NodeGraphManager.OutputDebugInfo )
				System.Diagnostics.Debug.WriteLine( "NodePort.OnCreate()" );

			IsInitialized = true;

			RaisePropertyChanged( "Model" );
		}

		public virtual void OnPreDestroy()
		{
			if( NodeGraphManager.OutputDebugInfo )
				System.Diagnostics.Debug.WriteLine( "NodePort.OnPreDestroy()" );
		}

		public virtual void OnPostDestroy()
		{
			if( NodeGraphManager.OutputDebugInfo )
				System.Diagnostics.Debug.WriteLine( "NodePort.OnPostDestroy()" );
		}

		public virtual void OnConnect( Connector connector )
		{
			if( NodeGraphManager.OutputDebugInfo )
				System.Diagnostics.Debug.WriteLine( "NodePort.OnConnect()" );
		}

		public virtual void OnDisconnect( Connector connector )
		{
			if( NodeGraphManager.OutputDebugInfo )
				System.Diagnostics.Debug.WriteLine( "NodePort.OnDisconnect()" );
		}

		public virtual void OnDeserialize()
		{
			if( NodeGraphManager.OutputDebugInfo )
				System.Diagnostics.Debug.WriteLine( "NodePort.OnDeserialize()" );

			IsInitialized = true;

			RaisePropertyChanged( "Model" );
		}

		#endregion // Callbacks

		#region Overrides IXmlSerializable

		public override void WriteXml( XmlWriter writer )
		{
			base.WriteXml( writer );

			//{ Begin Creation info : You need not deserialize this block in ReadXml().
			// These are automatically serialized in Node.ReadXml().
			writer.WriteAttributeString( "ViewModelType", ViewModel.GetType().AssemblyQualifiedName );
			writer.WriteAttributeString( "Owner", Owner.Guid.ToString() );
			writer.WriteAttributeString( "IsInput", IsInput.ToString() );
			//} End Creation Info.

			writer.WriteAttributeString( "Name", Name );
			writer.WriteAttributeString( "DisplayName", DisplayName );
			writer.WriteAttributeString( "AllowMultipleInput", AllowMultipleInput.ToString() );
			writer.WriteAttributeString( "AllowMultipleOutput", AllowMultipleOutput.ToString() );
			writer.WriteAttributeString( "IsPortEnabled", IsPortEnabled.ToString() );
			writer.WriteAttributeString( "IsEnabled", IsEnabled.ToString() );
		}

		public override void ReadXml( XmlReader reader )
		{
			base.ReadXml( reader );

			Name = reader.GetAttribute( "Name" );
			DisplayName = reader.GetAttribute( "DisplayName" );
			AllowMultipleInput = bool.Parse( reader.GetAttribute( "AllowMultipleInput" ) );
			AllowMultipleOutput = bool.Parse( reader.GetAttribute( "AllowMultipleOutput" ) );
			IsPortEnabled = bool.Parse( reader.GetAttribute( "IsPortEnabled" ) );
			IsEnabled = bool.Parse( reader.GetAttribute( "IsEnabled" ) );
		}

		#endregion // Overrides IXmlSerializable
	}
}
