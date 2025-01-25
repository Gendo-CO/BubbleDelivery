using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameSelectableScript : MonoBehaviour
{
	[SerializeField] protected Collider _coll;

	public bool Selected
	{
		get => _selected;
		set
		{
			if (!_selectable) return;
			_selected = value;
			OnSelect();
		}
	}
	[SerializeField] protected bool _selected = false;

	public bool Hovered
	{
		get => _hovered;
		set
		{
			if (!_selectable) return;
			_hovered = value;
			OnHover();
		}
	}
	[SerializeField] protected bool _hovered = false;

	public bool Selectable
	{
		get => _selectable;
		set
		{
			_selectable = value;
			_coll.enabled = _selectable;

			if (_hovered)
			{
				Hovered = false;
			}
			if (_selected)
			{
				//Selected = false;
			}
		}
	}
	[SerializeField] protected bool _selectable = false;

	private void Awake()
	{
		GameSelectableManager.Meh += OnSelect;
	}

	private void OnDestroy()
	{
		GameSelectableManager.Meh -= OnSelect;
	}

	protected abstract void OnSelect();
	protected abstract void OnHover();
}

public static class GameSelectableManager
{
    public static event Action Meh;

	//private IEnumerator GameLoop()
	//{
	//	Sele
	//	while (true)
	//	{

	//	}
	//}
}
