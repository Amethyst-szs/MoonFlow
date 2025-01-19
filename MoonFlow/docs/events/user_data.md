---
tags:
  - event_graph
---
# Event Nodes - User Data
Every node in your event flowchart can have additional data tied to it, called User Data. This data is stored in your graph's `.mfgraph` metadata, which means that it doesn't add any additional data to your actual event archives. However, it can be a super helpful tool to help organize and clearly communicate what your event graphs are doing.

To access a node's user data, press the small bubble located on the bottom-right corner of each node.

## Comments
The first and simplest option is the ability to set a comment. This will be displayed right below the nodes, and can be any text. Just like when working with regular code, comments are always helpful for weird or unintuitive nodes to help explain their purpose. When writing a comment, try to keep it short and sweet while still informative to help developers quickly grasp the meaning.

Also remember, it is always okay to not leave a comment if something doesn't need one! Take this demo start node for example:
![Demo Start Node](../asset/n4.png)

This node has one very simple purpose, start a demo/cutscene. Leaving a comment here would be redundant, since the meaning of the node is clearly denoted by its type.

## Colors
By default, every node is given a color based on its type. Dialogue and text nodes are blue, camera nodes are purple, player nodes are red, etc. However, the user data allows you to define a custom color!

What reasons would you want to define a custom color? Think of it as an organization tool one step below [groups](organization.md), where alike nodes can be given the same color to indicate their relationship to one another. On the opposite side of the coin, you can also use colors to make individual important nodes stand out at a glance!

There aren't any ground rules with colors, just make sure there is a reason for using a custom color and try to keep it intuitive.

## Search Tags
Search tags are a list of keywords used to distinguish nodes when searching. *As of right now, the node search system has not been fully implemented. Defining search tags has minimal use currently.*