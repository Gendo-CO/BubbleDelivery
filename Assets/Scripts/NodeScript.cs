using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeScript : GameSelectableScript
{
    public NodeScript[] Neighbors;
    public readonly List<(NodeScript, LineRenderer)> Paths = new();

	public BuildingScript Building => _building;
	private BuildingScript _building;

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

		if (_building == null)
		{
			_building = GetComponent<BuildingScript>();
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
