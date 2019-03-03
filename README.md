# NodeGraph
한국분은 [An introduction for WPF NodeGraph( Korean )](https://github.com/lifeisforu/NodeGraph/wiki/An-introduction-for-WPF-NodeGraph(-Koeran-)) 를 참고하세요.

WPF control librarty for node graph.

This library is inspired by a BlueprintEditor of UnrealEngine4.

A node can be divided as 3 parts, shown in below image; Node itself, FlowPorts, PropertyPorts.

![](/Documents/Images/NodeParts.png)

* A FlowPort is used for specifying a execution flow between two nodes. The connection can be made for a FlowPort with different direction. For example, an input FlowPort can be connected with an output FlowPort.
* A PropertyPort is used for specifying data-transfer between two nodes. The connection can be made for a PropertyPort with differenct direction. For example, an input PropertyPort can be connected with an output PropertyPort.

## Features

This provide below features:

* Creating/Destroying FlowChart.
* Creating/Destroying Node.
* Custom Node ViewModel and Styling.
* Creating/Destroying NodeFlowPort.
* Custom NodeFlowPort ViewModel and Styling.
* Supporting PropertyEditor for default types( bool, byte, short, int, long, float, double, string, enum, Media.Color ).
* Creating/Destroying NodePropertyPort.
* Custom NodePropertyPort ViewModel and Styling.
* Creating/Destroying Connector.
* Custom Connector ViewModel and Styling.
* Supporting Zoom & Pan.
* Supporting miscellaneous selection mode.
* Supporting History( undo/redo ).
* Serialization/Deserialization for XML.

## Controlling

### Connection

Drag-Drop between ports : Connects.
Ctrl + Left on port : Disconnects.

### Selection

Left : Select node and deselect all.
Ctrl + Left : XOR selection.
Shift + Left : Additive selection.
Alt + Left : Subtractive selection.

Ctrl + Left Dragging : XOR selection.
Shift + Left Dragging : Additive selection.
Alt + Left Dragging: Subtractive selection.

Ctrl + "A" : Select all nodes.

### Deletion

 "Delete" : Delete selected nodes.

### Zoom & Pan

"F" : Focus selected nodes.
"A" : Focus all nodes.
Right Dragging on flowchart : Panning.

## Class Diagram

NodeGraph supports MVVM( Model-View-ViewModel ) pattern.

![](https://github.com/lifeisforu/NodeGraph/raw/master/Documents/Images/NodeGraph_ClassDiagram.png)

All Model classes have their own attribute. I'll explain it later, all attributes could override ViewModel.

If you don't need special ViewModel or View, you can create node with basic appearances and behaviors just by adding attributes.

## Creating FlowChartView

Creating FlowChartView is start by adding NodeGraph.dll assembly as your project's reference.

![](https://github.com/lifeisforu/NodeGraph/raw/master/Documents/Images/NodeGraph_Reference.png)

Then, you can add a namespace in XAML of a Visual element. As you can see above, Model, View, ViewModel namespaces exsit. So you should add namespace for View. In my case, I have added name of "ngv". And then you can add "FlowChartView".

<pre>
"NodeGraphSamples/MainWindow.xaml"

<code>&lt;Window x:Class="NodeGraphSamples.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:local="clr-namespace:NodeGraphSamples"
	xmlns:ngv="clr-namespace:NodeGraph.View;assembly=NodeGraph"
        mc:Ignorable="d"
        Title="MainWindow" Height="450" Width="800"&gt;
    &lt;Grid&gt;
	&lt;ngv:FlowChartView DataContext="{Binding Path=FlowChartViewModel, 
                              RelativeSource={RelativeSource AncestorType={x:Type local:MainWindow}}}"/&gt;
    &lt;/Grid&gt;
&lt;/Window&gt;</code></pre>

## Creating and binding FlowChart

All Model instances in NodeGrpah could be created only by NodeGraphManager.

<pre>
"NodeGraphSamples/MainWindow.xaml.cs"

<code>public NodeGraph.ViewModel.FlowChartViewModel FlowChartViewModel
{
	get { return ( NodeGraph.ViewModel.FlowChartViewModel )GetValue( FlowChartViewModelProperty ); }
	set { SetValue( FlowChartViewModelProperty, value ); }
}
public static readonly DependencyProperty FlowChartViewModelProperty =
	DependencyProperty.Register( "FlowChartViewModel", typeof( NodeGraph.ViewModel.FlowChartViewModel ), 
	typeof( MainWindow ), new PropertyMetadata( null ) );

[...]

private void MainWindow_Loaded( object sender, RoutedEventArgs e )
{
	NodeGraph.Model.FlowChart flowChart = NodeGraph.NodeGraphManager.CreateFlowChart( 
		false, Guid.NewGuid(), typeof( NodeGraph.Model.FlowChart ) );
	FlowChartViewModel = flowChart.ViewModel;

	[ ... ]
}</code></pre>

If you create FlowChart instance, FlowChartViewModel is created automatically with it. So you can bind it.

NodeGraphManager.CreateFlowChart() is defined as below.

<pre>
"NodeGraph/NodeGraphManager.cs"

<code>/// &lt;summary&gt;
/// Create FlowChart with FlowChartViewModel.
/// &lt;/summary&gt;
/// &lt;param name="isDeserializing"&gt;Is in deserializing routine? 
/// If it is true, OnCreate() callback will not be called, otherwise OnPostLoad will be called.&lt;/param&gt;
/// &lt;param name="guid"&gt;Guid of this FlowChart.&lt;/param&gt;
/// &lt;param name="flowChartModelType"&gt;Type of FlowChart to be created.&lt;/param&gt;
/// &lt;returns&gt;Created FlowChart instance&lt;/returns&gt;
public static FlowChart CreateFlowChart( bool isDeserializing, Guid guid, Type flowChartModelType )</code></pre>

About all other parameters, I'll explain them later in other articles. Let's look third parameter. It specifices type of FlowChart. It is important, becuase it's attribute determines ViewModel of FlowChart.

For example, a basic FlowChart class is defined as below.

<pre>
"NodeGraph/Model/FlowChart.cs"

<code>[FlowChart()]
public class FlowChart : ModelBase
{
    [ ... ]
}
</code></pre>

FlowChartAttribute class is defined as below.

<pre>
"NodeGraph/Model/FlowChartAttribute.cs"

<code>
[AttributeUsage( AttributeTargets.Class )]
public class FlowChartAttribute : Attribute
{
	public Type ViewModelType = typeof( FlowChartViewModel );

	public FlowChartAttribute()
	{
		if( !typeof( FlowChartViewModel ).IsAssignableFrom( ViewModelType ) )
			throw new ArgumentException( "ViewModelType of FlowChartAttribute must be subclass of FlowChartViewModel" );
	}
}</code></pre>

If you want, you can override ViewModelType. CreateFlowChart() will get ViewModel from the attribute.

By now, you can see an empty FlowChartView.

![](https://github.com/lifeisforu/NodeGraph/raw/master/Documents/Images/FlowChart_Empty.png)

## Creating Node

Now, let's create nodes. By now, we don't have UI which could create nodes. So, we will create node with ContextMenu.

First, add ContextMenu-related event handlers to MainWindow.

<pre>
"NodeGraphSamples/MainWindow.xaml.cs"

<code>private void MainWindow_Loaded( object sender, RoutedEventArgs e )
{
	[ ... ]

	NodeGraphManager.BuildFlowChartContextMenu += NodeGraphManager_BuildFlowChartContextMenu;
	NodeGraphManager.BuildNodeContextMenu += NodeGraphManager_BuildNodeContextMenu;
	NodeGraphManager.BuildFlowPortContextMenu += NodeGraphManager_BuildFlowPortContextMenu;
	NodeGraphManager.BuildPropertyPortContextMenu += NodeGraphManager_BuildPropertyPortContextMenu;

	[ ... ]
}</code></pre>

Then, when NodeGraphManager_BuildFlowChartContextMenu event invoked, add menu items to ContextMenu.

<pre>
"NodeGraphSamples/MainWindow.xaml.cs"

<code>List&lt;Type&gt; _NodeTypes = new List&lt;Type&gt;()
{
	typeof( Model.AutoOutputFlow ),
	typeof( Model.AutoInOutFlow ),
	typeof( Model.DynamicOutputFlow ),
	typeof( Model.AutoNodeProperty ),
	typeof( Model.DynamicNodeProperty ),
};

private Point _ContextMenuLocation;

private bool NodeGraphManager_BuildFlowChartContextMenu( object sender, BuildContextMenuArgs args )
{
	ItemCollection items = args.ContextMenu.Items;

	_ContextMenuLocation = args.ModelSpaceMouseLocation;

	items.Clear();

	foreach( var nodeType in _NodeTypes )
	{
		MenuItem menuItem = new MenuItem();

		var NodeAttrs = nodeType.GetCustomAttributes( typeof( NodeAttribute ), false ) as NodeAttribute[];
		if( 1 != NodeAttrs.Length )
			throw new ArgumentException( string.Format( "{0} must have NodeAttribute", nodeType.Name ) );

		menuItem.Header = "Create " + NodeAttrs[ 0 ].Header;
		menuItem.CommandParameter = nodeType;
		menuItem.Click += FlowChart_ContextMenuItem_Click;
		items.Add( menuItem );
	}

	return ( 0 < items.Count );
}
</code></pre>

In above code snippets, types in _NodeTypes are pre-defined nodes I have been created. The mechanism is simple. While iterating _NodeTypes, get NodeAttribute attribute from each type. NodeAttribute contains appearances info of the node, among them, select "Header" field and set it as Header of MenuItem. And pass node's type to CommandParameter. It is to create a node with the type, when we click the menu item. _ContextMenuLocation is used for position of the node that will be created.

Now, you can see ContextMenu when you click mouse right button.

![](https://github.com/lifeisforu/NodeGraph/raw/master/Documents/Images/FlowChart_ContextMenu_Open.png)

What will happen, if you click "Create AutoOutputFlow" item. Let's find out.

<pre>
"NodeGraphSamples/MainWindow.xaml.cs"

<code>protected virtual void FlowChart_ContextMenuItem_Click( object sender, RoutedEventArgs e )
{
	MenuItem menuItem = sender as MenuItem;
	Type nodeType = menuItem.CommandParameter as Type;

	NodeGraph.View.FlowChartView flowChartView = FlowChartViewModel.View;

	Point nodePos = flowChartView.ZoomAndPan.MatrixInv.Transform(
		new Point( _ContextMenuLocation.X, _ContextMenuLocation.Y ) );

	Node node = NodeGraphManager.CreateNode(
		false, Guid.NewGuid(), FlowChartViewModel.Model, nodeType, nodePos.X, nodePos.Y, 0 );
}</code></pre>

As mentioned earlier, "ALL" Model instances must be created by NodeGraphManager. In here, we could call NodeGraphManager.CreateNode() method.

This method is defined as below.

<pre>
"NodeGraph/NodeGraphManager.cs"

<code>
/// &lt;summary&gt;
/// Create Node with NodeViewModel.
/// &lt;/summary&gt;
/// &lt;param name="isDeserializing"&gt;Is in deserializing routine? 
/// If it is true, OnCreate() callback will not be called, otherwise OnPostLoad will be called.
/// If it is true, Node's attribute will not be evaluated. That means flows and properties will not be created automatically by attributes.
/// All flows and properties will be created during deserialization process.&lt;/param&gt;
/// &lt;param name="guid"&gt;Guid for this Node.&lt;/param&gt;
/// &lt;param name="flowChart"&gt;Owner FlowChart.&lt;/param&gt;
/// &lt;param name="nodeType"&gt;Type of this node.&lt;/param&gt;
/// &lt;param name="x"&gt;Location along X axis( Canvas.Left ).&lt;/param&gt;
/// &lt;param name="y"&gt;Location along Y axis( Canvas.Top )&lt;/param&gt;
/// &lt;param name="ZIndex"&gt;Z index( Canvas.ZIndex ).&lt;/param&gt;
/// &lt;param name="nodeViewModelTypeOverride"&gt;NodeViewModel to override.&lt;/param&gt;
/// &lt;param name="flowPortViewModelTypeOverride"&gt;FlowPortViewModel to override.&lt;/param&gt;
/// &lt;param name="propertyPortViewModelTypeOverride"&gt;PropertyPortViewmodel to override.&lt;/param&gt;
/// &lt;returns&gt;Created node instance.&lt;/returns&gt;
public static Node CreateNode( bool isDeserializing, Guid guid, FlowChart flowChart, 
    Type nodeType, double x, double y, int ZIndex,
    Type nodeViewModelTypeOverride = null, Type flowPortViewModelTypeOverride = null, 
    Type propertyPortViewModelTypeOverride = null )</code></pre>

In this method, it has not only Node type but also XXXOverride types. Becuase I predict that, in the case of Node, ViewModel of Nodes are frequently replaced with other type of ViewModel, so I make additional ViewModel types. About the exmaple of this case, I will explain it later in other articles.

In above code snippets, to determine node's lcoation, you can see that I used a MatrixInv. Becuase mouse position is in ViewSpace, we must transform it to model space. These codes are a bit messy, in later, I have a plan to add some method like NodeGraphManager.CreateNodeViewSpace().

Anyway, if you click "Create AutoOutputFlow", you can see that a node named "AutoOutputFlow" will be created.

![](https://github.com/lifeisforu/NodeGraph/raw/master/Documents/Images/FlowChart_ContextMenu_Click.png)

Let's see an implementation of this class.

<pre>
"NodeGraphSample/Model/AutoOutputFlow"

<code>[Node()]
[NodeFlowPort( "Output", "", false )]
public class AutoOutputFlow : Node
{
	#region Constructor

	/// <summary>
	/// Never call this constructor directly. Use Node.Create() method.
	/// </summary>
	public AutoOutputFlow( Guid guid, FlowChart flowChart ) : base( guid, flowChart )
	{
		Header = "AutoInOutFlow";
		HeaderBackgroundColor = Brushes.Maroon;
	}

	#endregion // Constructor
}</code></pre>

So Simple!!! There are two attributes and one construtor. You just need to define a class with attributes.

In Constructor you can specify Header, HeaderBackgroundColor, HeaderFontColor, and in NodeAttribute you can specify ViewModel.

<pre>
"NodeGraph/Model/NodeAttribute"

<code>[AttributeUsage( AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum )]
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
</code></pre>

## Creating FlowPort

There is an output FlowPort in AutoOutputFlow node. It is automatically added by adding attribute like below.

<pre>
"NodeGraph/Model/NodeFlowPortAttribute.cs"

<code>[AttributeUsage( AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum, AllowMultiple = true ) ]
public class NodeFlowPortAttribute : NodePortAttribute
{
	public Type ViewModelType = typeof( NodeFlowPortViewModel );

	public NodeFlowPortAttribute( string name, string displayName, bool isInput ) : base( displayName, isInput )
	{
		Name = name;
		AllowMultipleInput = true;
		AllowMultipleOutput = false;

		if( !typeof( NodeFlowPortViewModel ).IsAssignableFrom( ViewModelType ) )
			throw new ArgumentException( "ViewModelType of NodeFlowPortAttribute must be subclass of NodeFlowPortViewModel" );
	}
}</code></pre>

NodeFlowPortAttribute is derived from NodePortAttribute. It defines common appearances and behavior of a port.

<pre>
"NodeGraph/Model/NodePortAttribute.cs"

<code>[AttributeUsage( AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum )]
public class NodePortAttribute : Attribute
{
	public string Name = string.Empty;
	public string DisplayName = string.Empty;
	public bool IsInput = false;
	public bool AllowMultipleInput = false;
	public bool AllowMultipleOutput = false;

	public NodePortAttribute( string displayName, bool isInput )
	{
		DisplayName = displayName;
		IsInput = isInput;
	}
}</code></pre>

## Creating PropertyPort

Now, Let's create a node with PropertyPort. If you click "Create AutoNodeProperty", below node will be created.

![](https://github.com/lifeisforu/NodeGraph/raw/master/Documents/Images/AutoNodeProperty.png)

As you can expect, this is acheived by adding simple attribute.

<pre>
"NodeGraphSamples/Model/AutoNodeProperty.cs"

<code>[Node()]
[NodeFlowPort( "Input", "", true )]
[NodeFlowPort( "Output", "", false )]
public class AutoNodeProperty : Node
{
	#region Input Properties

	[NodePropertyPort( "Input 0", typeof( double ), true, DefaultValue = 0.0 )]
	public double InputValue0;

	[NodePropertyPort( "Input 1", typeof( double ), true, DefaultValue = 0.0 )]
	public double InputValue1;

	#endregion // Input Properteis

	#region Output Properties

	[NodePropertyPort( "Output 0", typeof( double ), false, DefaultValue = 0.0 )]
	public object OutputValue0;

	[NodePropertyPort( "Output 1", typeof( double ), false, DefaultValue = 0.0 )]
	public object OutputValue1;

	[NodePropertyPort( "Output 2", typeof( double ), false, DefaultValue = 0.0 )]
	public object OutputValue2;

	#endregion // Output Properties

	#region Constructor

	/// <summary>
	/// Never call this constructor directly. Use Node.Create() method.
	/// </summary>
	public AutoNodeProperty( Guid guid, FlowChart flowChart, bool allowCircularConnection ) : base( guid, flowChart, allowCircularConnection )
	{
		Header = "AutoInOutFlow";
		HeaderBackgroundColor = Brushes.DarkBlue;
	}

	#endregion // Constructor
}</code></pre>

NodePropertyPortAttribute is derived from NodePortAttribute. And it is defined as below.

<pre>
"NodeGraphSamples/Model/NodePropertyPortAttribute.cs"

<code>[AttributeUsage( AttributeTargets.Field | AttributeTargets.Property )]
public class NodePropertyPortAttribute : NodePortAttribute
{
	public Type Type;
	public Type ViewModelType = typeof( NodePropertyPortViewModel );
	public object DefaultValue;

	public NodePropertyPortAttribute( string displayName, Type type, bool isInput ) : base( displayName, isInput )
	{
		Type = type;
		IsInput = isInput;
		AllowMultipleInput = false;
		AllowMultipleOutput = true;

		if( !typeof( NodePropertyPortViewModel ).IsAssignableFrom( ViewModelType ) )
			throw new ArgumentException( "ViewModelType of NodePropertyAttribute must be subclass of NodePropertyPortViewModel" );
	}
}</code></pre>

## Creating FlowPort dynamically

By now, I have introduced about static creation of FlowPort by using attributes. But you can create a FlowPort dynamically. If you click "Create DynmaicOutputFlow", below node will be created.

![](https://github.com/lifeisforu/NodeGraph/raw/master/Documents/Images/DynmaicOutputFlow.png)

Two FlowPorts are statically created by attributes, and 1 FlowPort( DynamicMulipleOutput ) is dynamically created.

<pre>
"NodeGraphSamples/Model/DynamicOutputFlow.cs"

<code>[Node( "DynamicOutputFlow", HeaderBackgroundColor = "Maroon", HeaderFontColor = "White" )]
[NodeFlowPort( "Input", "AutoInput", true )]
[NodeFlowPort( "Output", "AutoOutput", false )]
public class DynamicOutputFlow : Node
{
	[ ... ]
	
	#region Overrides Node

	public override void OnCreate()
	{
		base.OnCreate();

		NodeGraphManager.CreateNodeFlowPort( false, Guid.NewGuid(), this, "Output2", "DynamicMultipleOutput", false, false, true );
	}

	#endregion // Overrides Node.
}</code></pre>

In OnCreate() method, you can see that NodeGraphManager.CreateNodeFlowPort() create a port. Most parameters are same with fields of attributes.

<pre>
"NodeGraph/NodeGraphManager.cs"

<code>/// &lt;summary&gt;
/// Create NodeFlowPort with NodeFlwoPortViewModel.
/// &lt;/summary&gt;
/// &lt;param name="isDeserializing"&gt;Is in deserializing routine? 
/// If it is true, OnCreate() callback will not be called, otherwise OnPostLoad will be called.&lt;/param&gt;
/// &lt;param name="guid"&gt;Guid for this port.&lt;/param&gt;
/// &lt;param name="node"&gt;Owner of this port.&lt;/param&gt;
/// &lt;param name="name"&gt;Name of port.&lt;/param&gt;
/// &lt;param name="displayName"&gt;Display name of port.&lt;/param&gt;
/// &lt;param name="isInput"&gt;Is input port?&lt;/param&gt;
/// &lt;param name="allowMultipleInput"&gt;Multiple inputs are allowed for this port?&lt;/param&gt;
/// &lt;param name="allowMultipleOutput"&gt;Multiple outputs are allowed for this port?&lt;/param&gt;
/// &lt;param name="portViewModelTypeOverride"&gt;ViewModelType to override.&lt;/param&gt;
/// &lt;returns&gt;Created NodeFlwoPort instance.&lt;/returns&gt;
public static NodeFlowPort CreateNodeFlowPort( bool isDeserializing, Guid guid, Node node, string name, 
    string displayName, bool isInput, bool allowMultipleInput, bool allowMultipleOutput, 
    Type portViewModelTypeOverride = null )</code></pre>

## Creating PropertyPort dynamically

PropertyPort can be also created dynamically. If you click "Create DynamicNodeProperty", below node will be created.

![](https://github.com/lifeisforu/NodeGraph/raw/master/Documents/Images/DynmaicNodeProperty.png)

One dynamic input PropertyNode, and one dynamic output PropertyNode are created.

<pre>
"NodeGraphSamples/Model/DynamicNodeProperty.cs"

<code>public struct MyStruct
{
	public bool Bool;
	public int Int32;
	public double Double;
};

[Node()]
[NodeFlowPort( "Input", "", true )]
[NodeFlowPort( "Output", "", false )]
public class DynamicNodeProperty : Node
{
	[ ... ]

	#region Overrides Node

	public override void OnCreate()
	{
		base.OnCreate();

		NodeGraphManager.CreateNodePropertyPort(
			false, Guid.NewGuid(), this, "Input1", "Dynamic Input 1", true, false, false, 
			typeof( double ), 0.0 );

		NodeGraphManager.CreateNodePropertyPort(
			false, Guid.NewGuid(), this, "Output2", "Dynamic Output 2", false, false, true, 
			typeof( MyStruct ), new MyStruct() );
	}

	#endregion // Overrides Node
}</code></pre>

You can see that NodeGraphManager.CreateNodePropertyPort() creates nodes.

<pre>
"NodeGraph/NodeGraphManager.cs"

<code>/// &lt;summary&gt;
/// Create PropertyPort with PropertyPortViewModel.
/// &lt;/summary&gt;
/// &lt;param name="isDeserializing"&gt;Is in deserializing routine? 
/// If it is true, OnCreate() callback will not be called, otherwise OnPostLoad will be called.&lt;/param&gt;
/// &lt;param name="guid"&gt;Guid for this port.&lt;/param&gt;
/// &lt;param name="node"&gt;Owner of this port.&lt;/param&gt;
/// &lt;param name="name"&gt;Name of port.&lt;/param&gt;
/// &lt;param name="displayName"&gt;Display name of port.&lt;/param&gt;
/// &lt;param name="isInput"&gt;Is input port?&lt;/param&gt;
/// &lt;param name="allowMultipleInput"&gt;Multiple inputs are allowed for this port?&lt;/param&gt;
/// &lt;param name="allowMultipleOutput"&gt;Multiple outputs are allowed for this port?&lt;/param&gt;
/// &lt;param name="valueType"&gt;Type of property value.&lt;/param&gt;
/// &lt;param name="defaultValue"&gt;Default property value.&lt;/param&gt;
// &lt;param name="portViewModelTypeOverride"&gt;ViewModelType to override.&lt;/param&gt;
/// &lt;returns&gt;Created NodePropertyPort instance.&lt;/returns&gt;
public static NodePropertyPort CreateNodePropertyPort( bool isDeserializing, Guid guid, Node node, string name, 
    string displayName, bool isInput, bool allowMultipleInput, bool allowMultipleOutput, 
    Type valueType, object defaultValue, Type portViewModelTypeOverride = null )</code></pre>


## Conclusion

Elements of NodeGraph are categorized by Model-View-ViewModel. ALL Model instances are created by NodeGraphManager, it also create ViewModel instances. And ViewModels determine their Views.

Model class specify type of ViewModel, and appearances info is determined statically by attributes or dynamically by calling methods of NodeGraphManager.
