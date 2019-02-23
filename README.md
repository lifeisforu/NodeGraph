# NodeGraph
한국분은 [Introduction For Korean] 를 참고하세요.

WPF control librarty for node graph.

This library is inspired by a BlueprintEditor of UnrealEngine4.

A node can be divided as 3 parts, shown in below image; Node itself, FlowPorts, PropertyPorts.

![](/Documents/Images/NodeParts.png)

* A FlowPort is used for specifying a execution flow between two nodes. The connection can be made for a FlowPort with different direction. For example, an input FlowPort can be connected with an output FlowPort.
* A PropertyPort is used for specifying data-transfer between two nodes. The connection can be made for a PropertyPort with differenct direction. For example, an input PropertyPort can be connected with an output PropertyPort.

Features
* Nodes are specified by "NodeAttribute" attribute.
* FlowPorts are statically created by "NodeFlowPortAttribute" attribute or dynamically created/destroyed in code.
* PropertyPorts are statically created by "NodePropertyPortAttribute" attribute or dynamiclly created/destroyed in code.


