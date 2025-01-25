using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeScript : GameSelectableScript
{
    public NodeScript[] Neighbors;
    public readonly List<(NodeScript, LineRenderer)> Paths = new();

	protected override void OnSelect()
	{
		foreach (var (_, line) in Paths)
		{
			line.gameObject.SetActive(Selected);
		}
	}

	protected override void OnHover()
	{
		// TODO: light up area
	}
}
