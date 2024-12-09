using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;

namespace MoonFlow.LMS.Msbt;

[GlobalClass]
public partial class TagMenuFocusNeighborBuilder : Node
{
    [Export]
    public Array<HBoxContainer> Rows = [];

    public override void _Ready()
    {
        // Assign default neighbors
        for (int i = 0; i < Rows.Count; i++)
            AssignNeighbors(Rows[i], i);
    }

    private void AssignNeighbors(HBoxContainer row, int rowIdx)
    {
        List<Button> children = [];

        // Get list of children that are of type Button
        foreach (var child in row.GetChildren())
            if (child.GetType().IsSubclassOf(typeof(Button)))
                children.Add((Button)child);

        for (int i = 0; i < children.Count; i++)
        {
            var child = children[i];

            NodePath prev;
            NodePath next;
            NodePath self = child.GetPathTo(child);

            if (i == 0)
            {
                prev = child.GetPathTo(Rows[ModN(rowIdx - 1, Rows.Count)].GetChildren().Last());

                if (children.Count != 1)
                    next = child.GetPathTo(children[ModN(i + 1, children.Count)]);
                else
                    next = child.GetPathTo(Rows[ModN(rowIdx + 1, Rows.Count)].GetChildren().First());
            }
            else if (i == children.Count - 1)
            {
                prev = child.GetPathTo(children[ModN(i - 1, children.Count)]);
                next = child.GetPathTo(Rows[ModN(rowIdx + 1, Rows.Count)].GetChildren().First());
            }
            else
            {
                prev = child.GetPathTo(children[i - 1]);
                next = child.GetPathTo(children[i + 1]);
            }

            child.FocusNeighborLeft = prev;
            child.FocusNeighborRight = next;
            child.FocusPrevious = prev;
            child.FocusNext = next;

            child.FocusNeighborTop = self;
            child.FocusNeighborBottom = self;
        }
    }

    private static int ModN(int x, int m) {
        return (x%m + m)%m;
    }
}
