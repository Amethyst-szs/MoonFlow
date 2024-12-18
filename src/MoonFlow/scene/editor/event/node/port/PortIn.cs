using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MoonFlow.Scene.EditorEvent;

[GlobalClass]
[ScenePath("res://scene/editor/event/node/port/port_in.tscn")]
[Icon("res://asset/material/graph/port.svg")]
public partial class PortIn : TextureRect
{
	public EventFlowNodeCommon Parent { get; private set; } = null;

	private readonly List<PortOut> IncomingList = [];

	private static readonly Color DefaultColor = Colors.LightSlateGray;
	private static readonly Shader Shader = GD.Load<Shader>("res://asset/shader/graph/graph_port_in.gdshader");

	public override void _Ready()
	{
		// Search upward for parent flow node
		Node nextParent = this;
		while (Parent == null)
		{
			nextParent = nextParent.GetParent();
			if (!IsInstanceValid(nextParent))
				return;
			
			var t1 = nextParent.GetType();
			var t2 = typeof(EventFlowNodeCommon);
			
			if (t1 == t2 || t1.IsSubclassOf(t2))
				Parent = nextParent as EventFlowNodeCommon;
		}

		// Setup shader and display
		Material = new ShaderMaterial();
		(Material as ShaderMaterial).Shader = Shader;

		UpdateDisplay();
	}

	public void AddIncoming(PortOut n)
	{
		if (!IsInstanceValid(n) || IncomingList.Contains(n))
			return;
		
		IncomingList.Add(n);
		UpdateDisplay();
	}

	public void RemoveIncoming(PortOut n)
	{
		if (!IsInstanceValid(n) || !IncomingList.Contains(n))
			return;
		
		IncomingList.Remove(n);
		UpdateDisplay();
	}

	private void UpdateDisplay()
	{
		// Get color list
		Color[] list = new Color[4];
		int listPos = 0;
		foreach (var connection in IncomingList)
		{
			if (listPos >= list.Length)
				break;
			
			var color = connection.PortColor;
			if (!list.Contains(color))
			{
				list[listPos] = color;
				listPos++;
			}
		}

		// Send color information to shader
		var shader = Material as ShaderMaterial;

		if (listPos == 0)
		{
			list[0] = DefaultColor;
			listPos++;
		}

		shader.SetShaderParameter("colors", list);
		shader.SetShaderParameter("color_count", listPos);
	}
}
