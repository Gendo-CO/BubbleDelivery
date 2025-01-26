using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubblePersonScript : GameSelectableScript
{
	public NodeScript From = null; // not null if we're moving
	public NodeScript To = null; // not null if we're moving
	public NodeScript On; // not null if we've stopped moving
	public readonly Queue<NodeScript> TravelingPath = new();

	private GameManager _gameMgr;
	public SpriteRenderer Renderer;
	public List<Sprite> SpriteAssets = new();

	public event Action<BubblePersonScript> OnPop;

	public bool IsPopped { get; private set; } = false;

	public bool HasBox
	{
		get => _hasBox;
		set
		{
			if (_hasBox == value || IsPopped) return;
			_hasBox = value;
			Renderer.sprite = SpriteAssets[_hasBox ? 1 : 0];
		}
	}
	private bool _hasBox;

	public float Speed = 1f;

	public void GiveRoute(IEnumerable<NodeScript> route)
	{
		TravelingPath.Clear();
		foreach (var node in route)
		{
			TravelingPath.Enqueue(node);
		}
	}

	public void Pop()
	{
		if (IsPopped) return;

		IsPopped = true;
		Renderer.sprite = SpriteAssets[2];

		OnPop?.Invoke(this);

		StartCoroutine(PopRoutine());
	}

	private IEnumerator PopRoutine()
	{
		float deathTimer = 3f;
		while (deathTimer > 0f)
		{
			const float DROP_SPEED = 0.1f;
			transform.position -= new Vector3(0f, Time.deltaTime * DROP_SPEED, 0f);
			deathTimer -= Time.deltaTime;
			yield return null;
		}
		Destroy(this);
	}

	private void Start()
	{
		if (On == null)
		{
			Debug.LogError("Starting node needs to be set on BubblePersonScript");
		}

		//transform.position = On.transform.position;
		_gameMgr = FindObjectOfType<GameManager>(true);
		//if (_gameMgr != null && !_gameMgr.AllBubblePeople.Contains(this))
		//{
		//	_gameMgr.AllBubblePeople.Add(this);
		//}
	}

	private void Update()
	{
		if (To != null)
		{
			Vector3 distance = To.transform.position - transform.position;
			if (distance.magnitude <= Speed)
			{
				transform.position = To.transform.position;

				if (To.Building != null)
				{
					To.Building.OnVisit(this);
				}

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

    private void OnCollisionEnter(Collision collision)
    {
		if (collision.gameObject.CompareTag("Bubble"))
		{

		}
    }

    //private void OnDestroy()
    //{
    //	if (_gameMgr != null && _gameMgr.AllBubblePeople.Contains(this))
    //	{
    //		_gameMgr.AllBubblePeople.Remove(this);
    //	}
    //}

    protected override void OnHover() { }
	protected override void OnSelect() { }
}
