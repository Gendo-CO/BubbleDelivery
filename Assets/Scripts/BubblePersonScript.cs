using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubblePersonScript : GameSelectableScript
{
	public NodeScript From = null; // not null if we're moving
	public NodeScript To = null; // not null if we're moving
	public NodeScript On; // not null if we've stopped moving
	public readonly Queue<NodeScript> TravelingPath = new();

	public float Speed = 1f;

	public void GiveRoute(IEnumerable<NodeScript> route)
	{
		TravelingPath.Clear();
		foreach (var node in route)
		{
			TravelingPath.Enqueue(node);
		}
	}

	private void Start()
	{
		if (On == null)
		{
			Debug.LogError("Starting node needs to be set on BubblePersonScript");
		}

		transform.position = On.transform.position;
	}

	private void Update()
	{
		if (To != null)
		{
			Vector3 distance = To.transform.position - transform.position;
			if (distance.magnitude <= Speed)
			{
				transform.position = To.transform.position;
				if (TravelingPath.TryDequeue(out var next))
				{
					From = To;
					To = next;
					return;
				}

				On = To;
				From = null;
				To = null;
				return;
			}

			Vector3 diff = To.transform.position - From.transform.position;
			diff = diff.normalized * Speed;
			transform.position += diff;
		}
		else if (TravelingPath.TryDequeue(out var newTo))
		{
			To = newTo;
			From = On;
			On = null;
		}
	}

	protected override void OnHover() => throw new System.NotImplementedException();
	protected override void OnSelect() => throw new System.NotImplementedException();
}
