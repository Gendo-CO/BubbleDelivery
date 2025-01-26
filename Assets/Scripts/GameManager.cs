using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Camera _mainCam;
	public readonly List<BubblePersonScript> AllBubblePeople = new();
	public readonly List<NodeScript> AllNodes = new();
	public readonly Queue<NodeScript> Route = new();

	private void Awake()
	{
		Application.targetFrameRate = 60;
	}

	private void Start()
	{
		AllNodes.AddRange(FindObjectsByType<NodeScript>(FindObjectsInactive.Include, FindObjectsSortMode.None));
		StartCoroutine(GameLoop());
	}

	private IEnumerator GameLoop()
    {
        GameSelectableScript _hovered = null;
        BubblePersonScript personToMove = null;
		NodeScript firstSelected = null;
		NodeScript lastSelected = null;
		RaycastHit info = default;

		while (true)
        {
		Restart:
			if (_hovered != null)
			{
				_hovered.Hovered = false;
				_hovered = null;
			}
			if (personToMove != null)
			{
				personToMove.Selected = false;
				personToMove = null;
			}
			firstSelected = null;
			if (lastSelected != null)
			{
				lastSelected.Selected = false;
				lastSelected = null;
			}
			Route.Clear();
			foreach (var node in AllNodes)
			{
				node.Selectable = true;
				node.Selected = false;
				node.Hovered = false;
				node.Selectable = false;
			}
			foreach (var bubblePerson in AllBubblePeople)
			{
				bubblePerson.Selectable = true;
			}

			// Selecting bubble person
			while (true)
			{
				yield return null;

				if (Input.GetMouseButtonDown(1))
				{
					goto Restart;
				}

				if (Physics.Raycast(_mainCam.ScreenPointToRay(Input.mousePosition), out info))
				{
					if (info.collider.gameObject.TryGetComponent(out GameSelectableScript gsc))
					{
						if (_hovered != gsc)
						{
							if (_hovered != null) _hovered.Hovered = false;
							_hovered = gsc;
							if (_hovered != null) _hovered.Hovered = true;
						}
					}
				}
				else
				{
					if (_hovered != null)
					{
						_hovered.Hovered = false;
						_hovered = null;
					}
				}

				if (_hovered == null) continue;

				if (Input.GetMouseButtonDown(0) && _hovered is BubblePersonScript bps)
				{
					personToMove = bps;
					personToMove.Selected = true;
					personToMove.Hovered = false;
					foreach (var bubblePerson in AllBubblePeople)
					{
						bubblePerson.Selectable = false;
					}
					_hovered = null;

					personToMove.TravelingPath.Clear();
					firstSelected = personToMove.On ?? personToMove.To;
					lastSelected = firstSelected;
					lastSelected.Selectable = true;
					lastSelected.Selected = true;
					foreach (var node in lastSelected.Neighbors)
					{
						node.Selectable = true;
					}

					break;
				}
			}

			// Adding nodes to route
			while (true)
			{
				yield return null;
				// Can happen if currently selected bubble person "pops" and is destroyed
				if (personToMove == null) goto Restart;

				if (Input.GetMouseButtonDown(1))
				{
					goto Restart;
				}

				if (Physics.Raycast(_mainCam.ScreenPointToRay(Input.mousePosition), out info))
				{
					if (info.collider.gameObject.TryGetComponent(out GameSelectableScript a))
					{
						if (_hovered != a)
						{
							if (_hovered != null) _hovered.Hovered = false;
							_hovered = a;
							if (_hovered != null) _hovered.Hovered = true;
						}
					}
				}
				else
				{
					if (_hovered != null)
					{
						_hovered.Hovered = false;
						_hovered = null;
					}
				}

				if (_hovered == null) continue;

				if (Input.GetMouseButtonDown(0) && _hovered is NodeScript ns && ns.Selectable)
				{
					if (ns == lastSelected)
					{
						personToMove.GiveRoute(Route);
						goto Restart;
					}

					lastSelected.Selected = false;

					lastSelected = ns;

					lastSelected.Selected = true;
					lastSelected.Hovered = false;

					foreach (var node in AllNodes)
					{
						node.Selectable = false;
					}
					_hovered = null;
					Route.Enqueue(lastSelected);

					lastSelected.Selectable = true;
					foreach (var neighbor in lastSelected.Neighbors)
					{
						neighbor.Selectable = true;
					}
				}
			}
		}
    }
}
