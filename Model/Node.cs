using NodeGraph.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Xml;

namespace NodeGraph.Model
{
	[Node()]
	public class Node : ModelBase
	{
		#region Fields

		public readonly FlowChart Owner;

		#endregion // Fields

		#region Properties

		protected NodeViewModel _ViewModel;
		public NodeViewModel ViewModel
		{
			get { return _ViewModel; }
			set
			{
				if( value != _ViewModel )
				{
					_ViewModel = value;
					RaisePropertyChanged( "ViewModel" );
				}
			}
		}

		protected string _Header;
		public string Header
		{
			get { return _Header; }
			set
			{
				if( value != _Header )
				{
					AddNodePropertyCommand( "Header", _Header, value );
					_Header = value;
					RaisePropertyChanged( "Header" );
				}
			}
		}

		protected SolidColorBrush _HeaderBackgroundColor = Brushes.Black;
		public SolidColorBrush HeaderBackgroundColor
		{
			get { return _HeaderBackgroundColor; }
			set
			{
				if( value != _HeaderBackgroundColor )
				{
					AddNodePropertyCommand( "HeaderBackgroundColor", _HeaderBackgroundColor, value );
					_HeaderBackgroundColor = value;
					RaisePropertyChanged( "HeaderBackgroundColor" );
				}
			}
		}

		protected SolidColorBrush _HeaderFontColor = Brushes.White;
		public SolidColorBrush HeaderFontColor
		{
			get { return _HeaderFontColor; }
			set
			{
				if( value != _HeaderFontColor )
				{
					AddNodePropertyCommand( "HeaderFontColor", _HeaderFontColor, value );
					_HeaderFontColor = value;
					RaisePropertyChanged( "HeaderFontColor" );
				}
			}
		}

		private bool _AllowEditingHeader = true;
		public bool AllowEditingHeader
		{
			get { return _AllowEditingHeader; }
			set
			{
				if( value != _AllowEditingHeader )
				{
					AddNodePropertyCommand( "AllowEditingHeader", _AllowEditingHeader, value );
					_AllowEditingHeader = value;
					RaisePropertyChanged( "AllowEditingHeader" );
				}
			}
		}

		private bool _AllowCircularConnection = false;
		public bool AllowCircularConnection
		{
			get { return _AllowCircularConnection; }
			set
			{
				if( value != _AllowCircularConnection )
				{
					AddNodePropertyCommand( "AllowCircularConnection", _AllowCircularConnection, value );
					_AllowCircularConnection = value;
					RaisePropertyChanged( "AllowCircularConnection" );
				}
			}
		}

		protected double _X = 0.0;
		public double X
		{
			get { return _X; }
			set
			{
				if( value != _X )
				{
					_X = value;
					RaisePropertyChanged( "X" );
				}
			}
		}

		protected double _Y = 0.0;
		public double Y
		{
			get { return _Y; }
			set
			{
				if( value != _Y )
				{
					_Y = value;
					RaisePropertyChanged( "Y" );
				}
			}
		}

		protected int _ZIndex = 1;
		public int ZIndex
		{
			get { return _ZIndex; }
			set
			{
				if( value != _ZIndex )
				{
					_ZIndex = value;
					RaisePropertyChanged( "ZIndex" );
				}
			}
		}

		protected ObservableCollection<NodeFlowPort> _InputFlowPorts = new ObservableCollection<NodeFlowPort>();
		public ObservableCollection<NodeFlowPort> InputFlowPorts
		{
			get { return _InputFlowPorts; }
			set
			{
				if( value != _InputFlowPorts )
				{
					_InputFlowPorts = value;
					RaisePropertyChanged( "InputFlowPorts" );
				}
			}
		}

		protected ObservableCollection<NodeFlowPort> _OutputFlowPorts = new ObservableCollection<NodeFlowPort>();
		public ObservableCollection<NodeFlowPort> OutputFlowPorts
		{
			get { return _OutputFlowPorts; }
			set
			{
				if( value != _OutputFlowPorts )
				{
					_OutputFlowPorts = value;
					RaisePropertyChanged( "OutputFlowPorts" );
				}
			}
		}

		protected ObservableCollection<NodePropertyPort> _InputPropertyPorts = new ObservableCollection<NodePropertyPort>();
		public ObservableCollection<NodePropertyPort> InputPropertyPorts
		{
			get { return _InputPropertyPorts; }
			set
			{
				if( value != _InputPropertyPorts )
				{
					_InputPropertyPorts = value;
					RaisePropertyChanged( "InputPropertyPorts" );
				}
			}
		}

		protected ObservableCollection<NodePropertyPort> _OutputPropertyPorts = new ObservableCollection<NodePropertyPort>();
		public ObservableCollection<NodePropertyPort> OutputPropertyPorts
		{
			get { return _OutputPropertyPorts; }
			set
			{
				if( value != _OutputPropertyPorts )
				{
					_OutputPropertyPorts = value;
					RaisePropertyChanged( "OutputPropertyPorts" );
				}
			}
		}

		#endregion // Properties

		#region Constructor

		/// <summary>
		/// Never call this constructor directly. Use GraphManager.CreateNode() method.
		/// </summary>
		public Node( Guid guid, FlowChart flowChart ) : base( guid )
		{
			Owner = flowChart;
		}

		#endregion // Constructor

		#region Destructor

		~Node()
		{
			
		}

		#endregion // Destructor

		#region Callbacks

		public virtual void OnCreate()
		{
			if( NodeGraphManager.OutputDebugInfo )
				System.Diagnostics.Debug.WriteLine( "Node.OnPreExecute()" );

			IsInitialized = true;

			RaisePropertyChanged( "Model" );
		}
		
		public virtual void OnPreExecute( Connector prevConnector )
		{
			if( NodeGraphManager.OutputDebugInfo )
				System.Diagnostics.Debug.WriteLine( "Node.OnPreExecute()" );
		}

		public virtual void OnExecute( Connector prevConnector )
		{
			if( NodeGraphManager.OutputDebugInfo )
				System.Diagnostics.Debug.WriteLine( "Node.OnExecute()" );
		}

		public virtual void OnPostExecute( Connector prevConnector )
		{
			if( NodeGraphManager.OutputDebugInfo )
				System.Diagnostics.Debug.WriteLine( "Node.OnPostExecute()" );
		}

		public virtual void OnPreDestroy()
		{
			if( NodeGraphManager.OutputDebugInfo )
				System.Diagnostics.Debug.WriteLine( "Node.OnPreDestroy()" );
		}

		public virtual void OnPostDestroy()
		{
			if( NodeGraphManager.OutputDebugInfo )
				System.Diagnostics.Debug.WriteLine( "Node.OnPostDestroy()" );
		}

		public virtual void OnDeserialize()
		{
			if( NodeGraphManager.OutputDebugInfo )
				System.Diagnostics.Debug.WriteLine( "Node.OnDeserialize()" );

			foreach( var port in InputFlowPorts )
			{
				port.OnDeserialize();
			}

			foreach( var port in OutputFlowPorts )
			{
				port.OnDeserialize();
			}

			foreach( var port in InputPropertyPorts )
			{
				port.OnDeserialize();
			}

			foreach( var port in OutputPropertyPorts )
			{
				port.OnDeserialize();
			}

			IsInitialized = true;

			RaisePropertyChanged( "Model" );
		}

		#endregion // Callbacks

		#region Overrides IXmlSerializable

		public override void WriteXml( XmlWriter writer )
		{
			base.WriteXml( writer );

			//{ Begin Creation info : You need not deserialize this block in ReadXml().
			// These are automatically serialized in FlowChart.ReadXml().
			writer.WriteAttributeString( "ViewModelType", ViewModel.GetType().FullName );
			writer.WriteAttributeString( "Owner", Owner.Guid.ToString() );
			//} End creation info.

			writer.WriteAttributeString( "Header", Header );
			writer.WriteAttributeString( "HeaderBackgroundColor", HeaderBackgroundColor.ToString() );
			writer.WriteAttributeString( "HeaderFontColor", HeaderFontColor.ToString() );
			writer.WriteAttributeString( "AllowEditingHeader", AllowEditingHeader.ToString() );

			writer.WriteAttributeString( "AllowCircularConnection", AllowCircularConnection.ToString() );

			writer.WriteAttributeString( "X", X.ToString() );
			writer.WriteAttributeString( "Y", Y.ToString() );
			writer.WriteAttributeString( "ZIndex", ZIndex.ToString() );

			writer.WriteStartElement( "InputFlowPorts" );
			foreach( var port in InputFlowPorts )
			{
				writer.WriteStartElement( "FlowPort" );
				port.WriteXml( writer );
				writer.WriteEndElement();
			}
			writer.WriteEndElement();

			writer.WriteStartElement( "OutputFlowPorts" );
			foreach( var port in OutputFlowPorts )
			{
				writer.WriteStartElement( "FlowPort" );
				port.WriteXml( writer );
				writer.WriteEndElement();
			}
			writer.WriteEndElement();

			writer.WriteStartElement( "InputPropertyPorts" );
			foreach( var port in InputPropertyPorts )
			{
				writer.WriteStartElement( "PropertyPort" );
				port.WriteXml( writer );
				writer.WriteEndElement();
			}
			writer.WriteEndElement();

			writer.WriteStartElement( "OutputPropertyPorts" );
			foreach( var port in OutputPropertyPorts )
			{
				writer.WriteStartElement( "PropertyPort" );
				port.WriteXml( writer );
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}

		public override void ReadXml( XmlReader reader )
		{
			base.ReadXml( reader );

			Header = reader.GetAttribute( "Header" );
			HeaderBackgroundColor = new SolidColorBrush(
				( Color )ColorConverter.ConvertFromString( reader.GetAttribute( "HeaderBackgroundColor" ) ) );
			HeaderFontColor = new SolidColorBrush( ( Color )ColorConverter.ConvertFromString( 
				reader.GetAttribute( "HeaderFontColor" ) ) );
			AllowEditingHeader = bool.Parse( reader.GetAttribute( "AllowEditingHeader" ) );

			AllowCircularConnection = bool.Parse( reader.GetAttribute( "AllowCircularConnection" ) );

			X = double.Parse( reader.GetAttribute( "X" ) );
			Y = double.Parse( reader.GetAttribute( "Y" ) );
			ZIndex = int.Parse( reader.GetAttribute( "ZIndex" ) );

			bool isInputFlowPortsEnd = false;
			bool isOutputFlowPortsEnd = false;
			bool isInputPropertyPortsEnd = false;
			bool isOutputPropertyPortsEnd = false;
			while( reader.Read() )
			{
				if( XmlNodeType.Element == reader.NodeType )
				{
					if( ( "PropertyPort" == reader.Name ) ||
						( "FlowPort" == reader.Name ) )
					{
						string prevReaderName = reader.Name;

						Guid guid = Guid.Parse( reader.GetAttribute( "Guid" ) );
						Type type = Type.GetType( reader.GetAttribute( "Type" ) );
						Type vmType = Type.GetType( reader.GetAttribute( "ViewModelType" ) );
						bool isInput = bool.Parse( reader.GetAttribute( "IsInput" ) );

						string ownerGuidString = reader.GetAttribute( "Owner" );
						Node node = NodeGraphManager.FindNode( Guid.Parse( ownerGuidString ) );

						if( "PropertyPort" == prevReaderName )
						{
							string name = reader.GetAttribute( "Name" );
							Type valueType = Type.GetType( reader.GetAttribute( "ValueType" ) );
							bool hasEditor = bool.Parse( reader.GetAttribute( "HasEditor" ) );

							NodePropertyPort port = NodeGraphManager.CreateNodePropertyPort(
								true, guid, node, isInput, valueType, null, name, hasEditor, vmType );
							port.ReadXml( reader );
						}
						else
						{
							NodeFlowPort port = NodeGraphManager.CreateNodeFlowPort(
								true, guid, node, isInput, vmType );
							port.ReadXml( reader );
						}
					}
						
				}

				if( reader.IsEmptyElement || ( XmlNodeType.EndElement == reader.NodeType ) )
				{
					if( "InputFlowPorts" == reader.Name )
					{
						isInputFlowPortsEnd = true;
					}
					else if( "OutputFlowPorts" == reader.Name )
					{
						isOutputFlowPortsEnd = true;
					}
					else if( "InputPropertyPorts" == reader.Name )
					{
						isInputPropertyPortsEnd = true;
					}
					else if( "OutputPropertyPorts" == reader.Name )
					{
						isOutputPropertyPortsEnd = true;
					}
				}

				if( isInputFlowPortsEnd && isOutputFlowPortsEnd && 
					isInputPropertyPortsEnd && isOutputPropertyPortsEnd )
				{
					break;
				}
			}
		}

		#endregion // Overrides IXmlSerializable

		#region History

		protected virtual void AddNodePropertyCommand( string propertyName, object prevValue, object newValue )
		{
			if( !IsInitialized )
			{
				return;
			}

			Owner.History.BeginTransaction( "Setting Property" );

			Owner.History.AddCommand( new History.NodePropertyCommand(
				propertyName, Guid, propertyName, prevValue, newValue ) );

			Owner.History.EndTransaction( false );
		}

		#endregion // History

		#region PropertyChanged

		public override void RaisePropertyChanged( string propertyName )
		{
			base.RaisePropertyChanged( propertyName );

			NodePropertyPort port = NodeGraphManager.FindNodePropertyPort( this, propertyName );
			if( null != port )
			{
				Type nodeType = GetType();

				PropertyInfo propertyInfo = nodeType.GetProperty( propertyName );
				if( null != propertyInfo )
				{
					port.Value = propertyInfo.GetValue( this );
				}
				else
				{
					FieldInfo fieldInfo = nodeType.GetField( propertyName );
					if( null != fieldInfo )
					{
						port.Value = fieldInfo.GetValue( this );
					}
				}				
			}
		}

		#endregion // PropertyChanged
	}
}
