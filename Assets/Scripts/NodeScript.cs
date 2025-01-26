using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeScript : GameSelectableScript
{
    public NodeScript[] Neighbors;
    public readonly List<(NodeScript, LineRenderer)> Paths = new();

	private void Awake()
	{
		if (_coll == null)
		{
			_coll = GetComponent<Collider>();
		}
		if (_coll == null)
		{
			_coll = GetComponentInChildren<Collider>(true);
		}
	}

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
