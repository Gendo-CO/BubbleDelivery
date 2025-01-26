using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Camera _mainCam;
	public readonly List<BubblePersonScript> AllBubblePeople = new();
	public readonly List<NodeScript> AllNodes = new();
	public readonly List<DeliverableHouse> NormalHouses = new();
	public readonly List<DeliverableHouse> PickupHouses = new();

	public readonly Queue<NodeScript> Route = new();

	[SerializeField] private BubblePersonScript _bubbleMailmanPrefab;
	[SerializeField] private NodeScript _postOfficeSpawnPoint;

	[SerializeField] private TextMeshProUGUI TimerText;
	[SerializeField] private float SecondsPerRound = 30f;
	[SerializeField] [Range(1.1f, 10f)] private float TargetPackagesExponent = 2f;

	public int Round = 1;
	public int DeliveryTargetAmount = 2;
	public float TimeLeftInSeconds = 30;
	private float _cachedTargetAmount = 1f;
	private int _pickupThreshold = 2;

	private Coroutine _gameLoop;
	private Coroutine _houseLoop;
	private Coroutine _spawnLoop;

	private void Awake()
	{
		Application.targetFrameRate = 60;
	}

	private void Start()
	{
		AllNodes.AddRange(FindObjectsByType<NodeScript>(FindObjectsInactive.Include, FindObjectsSortMode.None));
		AllBubblePeople.AddRange(FindObjectsByType<BubblePersonScript>(FindObjectsInactive.Include, FindObjectsSortMode.None));
		_gameLoop = StartCoroutine(GameLoop());
		_houseLoop = StartCoroutine(HouseLoop());
		_spawnLoop = StartCoroutine(SpawnLoop());
	}

	private void Update()
	{
		if (TimeLeftInSeconds <= 0f) return;

		TimeLeftInSeconds -= Time.deltaTime;

		if (TimeLeftInSeconds <= 0f)
		{
			TimeLeftInSeconds = 0f;
			StopCoroutine(_gameLoop);
			StopCoroutine(_houseLoop);
			StopCoroutine(_spawnLoop);

			// TODO: more game over stuff

			return;
		}

		if (DeliveryTargetAmount <= 0)
		{
			Round++;
			_cachedTargetAmount *= TargetPackagesExponent;
			DeliveryTargetAmount = (int)_cachedTargetAmount;
			TimeLeftInSeconds += SecondsPerRound;
			_pickupThreshold = Round + 1;
		}

		TimerText.text = $"Round {Round}\nDeliveries Remaining: {DeliveryTargetAmount}\nTime Left: {TimeLeftInSeconds:F2}";
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
					var trans = info.collider.transform;
					if (trans.parent != null && trans.parent.TryGetComponent(out GameSelectableScript gsc))
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
						if (node == null) continue;
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
				if (personToMove == null || personToMove.IsPopped) goto Restart;

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
						if (neighbor == null) continue;
						neighbor.Selectable = true;
					}
				}
			}
		}
    }

	private IEnumerator HouseLoop()
	{
		while (true)
		{
			if (PickupHouses.Count < _pickupThreshold && NormalHouses.Count > 0)
			{
				yield return new WaitForSeconds(3f);
				int iter = Random.Range(0, NormalHouses.Count);
				var pickupHouse = NormalHouses[iter];
				NormalHouses.RemoveAt(iter);
				PickupHouses.Add(pickupHouse);
				pickupHouse.HasPackageToGive = true;
			}

			yield return null;
		}
	}

	private IEnumerator SpawnLoop()
	{
		while (true)
		{
			yield return new WaitForSeconds(10f);

			var newMailman = Instantiate(_bubbleMailmanPrefab);
			Vector3 start = _postOfficeSpawnPoint.transform.position + new Vector3(0, 100f, 0);
			newMailman.transform.position = start;
			newMailman.On = _postOfficeSpawnPoint;
			const float DESCEND_TIME_IN_SECS = 2f;
			float timer = 0f;
			// New mailman can pop if it lands on top of another mailman during its descent
			while (timer < DESCEND_TIME_IN_SECS && !newMailman.IsPopped)
			{
				float t = timer / DESCEND_TIME_IN_SECS;
				newMailman.transform.position = Vector3.Lerp(start, _postOfficeSpawnPoint.transform.position, Mathf.Sqrt(t));
				timer += Time.deltaTime;
				yield return null;
			}

			if (newMailman.IsPopped) continue;
			newMailman.transform.position = _postOfficeSpawnPoint.transform.position;
			newMailman.OnPop += MailmanPopped;
			AllBubblePeople.Add(newMailman);
		}
	}

	private void MailmanPopped(BubblePersonScript bps)
	{
		AllBubblePeople.Remove(bps);
		bps.Selectable = true;
		bps.Selected = false;
		bps.Hovered = false;
		bps.Selectable = false;
		bps.OnPop -= MailmanPopped;
	}
}
