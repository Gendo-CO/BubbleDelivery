using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
	[SerializeField] [Range(1.1f, 10f)] private float TargetPackagesIncrease = 2f;

	public int Round = 1;
	public int DeliveryTargetAmount = 2;
	public float TimeLeftInSeconds = 30;
	private float _cachedTargetAmount = 2f;
	private int _pickupThreshold = 2;

	private Coroutine _gameLoop;
	private Coroutine _houseLoop;
	private Coroutine _spawnLoop;

	private readonly HashSet<BubblePersonScript> _toPop = new();
	[SerializeField] private float BubbleDudeRadius = 1f;
	[SerializeField] private float DudeSpawnPeriodInSecs = 3f;
	[SerializeField] private float DescentTimeInSecs = 2f;
	[SerializeField] private float DescentStartingHeight = 100f;

	private BubblePersonScript _personToMove = null;
	private GameSelectableScript _hovered = null;

	[SerializeField] private TextMeshProUGUI _gameOverTextbox;
	[SerializeField] private Image _titleCardImage;

	private void Awake()
	{
		Application.targetFrameRate = 60;
	}

	private void Start()
	{
		AllNodes.AddRange(FindObjectsByType<NodeScript>(FindObjectsInactive.Include, FindObjectsSortMode.None));
		AllBubblePeople.AddRange(FindObjectsByType<BubblePersonScript>(FindObjectsInactive.Include, FindObjectsSortMode.None));
		foreach (var bubblePerson in AllBubblePeople)
		{
			if (bubblePerson.On == null)
			{
				bubblePerson.On = _postOfficeSpawnPoint;
			}
			bubblePerson.transform.position = bubblePerson.On.transform.position;
		}
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

			for (int i = AllBubblePeople.Count - 1; i >= 0; i--)
			{
				OnDudePopped(AllBubblePeople[i]);
			}

			_gameOverTextbox.text = $"GAME OVER!\nYou made it to Round {Round}";
			_titleCardImage.gameObject.SetActive(true);

			return;
		}

		_toPop.Clear();
		for (int i = 0; i < AllBubblePeople.Count; i++)
		{
			var a = AllBubblePeople[i];
			for (int j = i + 1; j < AllBubblePeople.Count; j++)
			{
				var b = AllBubblePeople[j];
				var diff = a.transform.position - b.transform.position;
				if (diff.magnitude <= BubbleDudeRadius)
				{
					_toPop.Add(a);
					_toPop.Add(b);
				}
			}
		}
		foreach (var dudeToPop in _toPop)
		{
			OnDudePopped(dudeToPop);
		}

		if (DeliveryTargetAmount <= 0)
		{
			Round++;
			_cachedTargetAmount += TargetPackagesIncrease;
			DeliveryTargetAmount = (int)_cachedTargetAmount;
			TimeLeftInSeconds += SecondsPerRound;
			_pickupThreshold = Round + 1;
		}

		TimerText.text = $"Round {Round}\nDeliveries Remaining: {DeliveryTargetAmount}\nTime Left: {TimeLeftInSeconds:F2}";
	}

	private void OnDudePopped(BubblePersonScript dudeToPop)
	{
		AllBubblePeople.Remove(dudeToPop);
		dudeToPop.Selectable = true;
		dudeToPop.Selected = false;
		dudeToPop.Hovered = false;
		dudeToPop.Selectable = false;
		dudeToPop.Pop();
	}

	private IEnumerator GameLoop()
    {
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
			if (_personToMove != null)
			{
				_personToMove.Selected = false;
				_personToMove = null;
			}
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
					_personToMove = bps;
					_personToMove.Selected = true;
					_personToMove.Hovered = false;
					foreach (var bubblePerson in AllBubblePeople)
					{
						bubblePerson.Selectable = false;
					}
					_hovered = null;

					_personToMove.TravelingPath.Clear();
					lastSelected = _personToMove.On ?? _personToMove.To;
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
				if (_personToMove == null || _personToMove.IsPopped) goto Restart;

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

				if (_hovered != null && _hovered is NodeScript ns && ns != lastSelected && ns.Selectable)
				{
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

				if (Input.GetMouseButtonUp(0))
				{
					_personToMove.GiveRoute(Route);
					goto Restart;
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
			yield return new WaitForSeconds(DudeSpawnPeriodInSecs);

			var newMailman = Instantiate(_bubbleMailmanPrefab);
			Vector3 start = _postOfficeSpawnPoint.transform.position + new Vector3(0, DescentStartingHeight, 0);
			newMailman.transform.position = start;
			newMailman.On = _postOfficeSpawnPoint;
			newMailman.Selectable = false;
			//const float DESCEND_TIME_IN_SECS = 2f;
			float timer = 0f;
			// New mailman can pop if it lands on top of another mailman during its descent
			while (timer < DescentTimeInSecs && !newMailman.IsPopped)
			{
				float t = timer / DescentTimeInSecs;
				newMailman.transform.position = Vector3.Lerp(start, _postOfficeSpawnPoint.transform.position, Mathf.Sqrt(t));

				for (int i = 0; i < AllBubblePeople.Count; i++)
				{
					var other = AllBubblePeople[i];
					if ((other.transform.position - newMailman.transform.position).magnitude <= BubbleDudeRadius)
					{
						newMailman.Pop();
						OnDudePopped(other);
						break;
					}
				}

				timer += Time.deltaTime;
				yield return null;
			}

			if (newMailman.IsPopped) continue;
			newMailman.transform.position = _postOfficeSpawnPoint.transform.position;
			//newMailman.OnPop += MailmanPopped;
			AllBubblePeople.Add(newMailman);
			newMailman.Selectable = _personToMove == null;
		}
	}
}
