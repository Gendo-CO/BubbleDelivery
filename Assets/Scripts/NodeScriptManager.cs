using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NodeScriptManager : MonoBehaviour
{
    // Singleton pattern
    private static NodeScriptManager Singleton = null;
	public LineRenderer LinePrefab;
	//public NodeScript[] AllNodes;
	readonly HashSet<NodeScript> AllNodes = new();
	private readonly List<LineRenderer> _renderers = new();
	public List<Material> Materials = new List<Material>();

	private void Awake()
	{
		if (Singleton != null)
		{
			Destroy(this);
			return;
		}

		Singleton = this;
	}

	private void Start()
	{
		var allInScene = FindObjectsByType<NodeScript>(FindObjectsInactive.Include, FindObjectsSortMode.None);
		if (allInScene is not null && allInScene.Length > 1) AddNodes(allInScene);
	}

	private void AddNodes(IEnumerable<NodeScript> nodesToAdd)
	{
		foreach (var curr in nodesToAdd)
		{
			if (!AllNodes.Add(curr)) continue;

			if (curr.Neighbors is not null)
			{
				//var neighbors = curr.Neighbors.Where(x => !AllNodes.Contains(x) || x.Neighbors is null || !x.Neighbors.Contains(curr));
				foreach (var neighbor in curr.Neighbors)
				{
					if (neighbor == null) continue;

					var newLine = Instantiate(LinePrefab, transform);
					newLine.positionCount = 2;
					newLine.SetPositions(new Vector3[] { curr.transform.position, neighbor.transform.position });
					if (neighbor.Neighbors is null || !neighbor.Neighbors.Contains(curr))
					{
						newLine.endWidth = newLine.startWidth * 0.1f;
					}
					_renderers.Add(newLine);
					curr.Paths.Add((neighbor, newLine));
				}
			}
		}
	}
}
