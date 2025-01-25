using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Camera _mainCam;
	private readonly List<BubblePersonScript> AllBubblePeople = new();
	private readonly List<NodeScript> AllNodes = new();

	private void Awake()
	{
		Application.targetFrameRate = 60;
	}

	private void Start()
	{
		StartCoroutine(GameLoop());
	}

	private IEnumerator GameLoop()
    {
        GameSelectableScript _hovered = null;
        BubblePersonScript personToMove = null;
		NodeScript lastSelected = null;
		Queue<NodeScript> route = new();
		RaycastHit info;

		while (true)
        {
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
					foreach (var bubblePerson in AllBubblePeople)
					{
						bubblePerson.Selectable = false;
					}
					personToMove.Hovered = false;
					_hovered = null;

					break;
				}
			}

			// Selecting route
			route.Clear();
			while (true)
			{
				yield return null;
				// Can happen if currently selected bubble person "pops" and is destroyed
				if (personToMove == null) goto Restart;

				if (Input.GetMouseButtonDown(1))
				{
					goto Restart;
				}

				if (personToMove.On != null)
				{
					foreach (var node in personToMove.On.Neighbors)
					{
						node.Selectable = true;
					}
				}
				else if (personToMove.To != null)
				{
					personToMove.To.Selectable = true;
					personToMove.From.Selectable = true;
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
					lastSelected = ns;
					lastSelected.Selected = true;
					foreach (var node in AllNodes)
					{
						node.Selectable = false;
					}
					lastSelected.Hovered = false;
					_hovered = null;
					route.Enqueue(lastSelected);

					foreach (var neighbor in lastSelected.Neighbors.Where(x => !route.Contains(x)))
					{
						neighbor.Selectable = true;
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

				if (Input.GetMouseButtonDown(0) && _hovered is NodeScript ns)
				{
					if (ns == lastSelected)
					{
						personToMove.GiveRoute(route);
						goto Restart;
					}
					else if (!ns.Selectable)
					{
						continue;
					}

					lastSelected = ns;
					lastSelected.Selected = true;
					foreach (var node in AllNodes)
					{
						node.Selectable = false;
					}
					lastSelected.Hovered = false;
					_hovered = null;
					route.Enqueue(lastSelected);

					foreach (var neighbor in lastSelected.Neighbors.Where(x => !route.Contains(x)))
					{
						neighbor.Selectable = true;
					}
				}
			}

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
			if (lastSelected != null)
			{
				lastSelected.Selected = false;
				lastSelected = null;
			}
			foreach (var node in AllNodes)
			{
				node.Selectable = false;
			}
			foreach (var bubblePerson in AllBubblePeople)
			{
				bubblePerson.Selectable = true;
			}
			foreach (var n in route)
			{
				n.Selected = false;
			}
			route.Clear();
		}
    }
}
