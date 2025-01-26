using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BuildingScript : MonoBehaviour
{
	public abstract KindOfBuilding BuildingType { get; }
	public abstract void OnVisit(BubblePersonScript bps);
}

public enum KindOfBuilding
{
	House,
	PostOffice,
}