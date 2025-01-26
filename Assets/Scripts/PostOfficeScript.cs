using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostOfficeScript : BuildingScript
{
	private GameManager _gameMgr;

	public override KindOfBuilding BuildingType => KindOfBuilding.PostOffice;

	private void Start()
	{
		_gameMgr = FindObjectOfType<GameManager>(true);
	}

	public override void OnVisit(BubblePersonScript bps)
	{
		if (bps.HasBox)
		{
			bps.HasBox = false;
			_gameMgr.DeliveryTargetAmount -= 1;
		}
	}
}
