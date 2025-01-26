using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliverableHouse : BuildingScript
{
    public GameObject mailIcon;
    public bool HasPackageToGive
    {
        get => _packageToGive;
        set
        {
            if (_packageToGive == value) return;
            _packageToGive = value;
            mailIcon.SetActive(_packageToGive);
        }
    }
    private bool _packageToGive = false;

    private GameManager _gameMgr;

	public override KindOfBuilding BuildingType => KindOfBuilding.House;

	private void Start()
	{
        _gameMgr = FindObjectOfType<GameManager>(true);
        if (_gameMgr != null)
        {
            _gameMgr.NormalHouses.Add(this);
        }
	}

	public override void OnVisit(BubblePersonScript bps)
    {
        if (HasPackageToGive && !bps.HasBox)
        {
            HasPackageToGive = false;
            bps.HasBox = true;
            _gameMgr.PickupHouses.Remove(this);
            _gameMgr.NormalHouses.Add(this);
        }
    }
}
