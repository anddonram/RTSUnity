using UnityEngine;
using System.Collections;
using RTS;

public class GameObjectList : MonoBehaviour
{

	public GameObject[] buildings;
	public GameObject[] units;

	public TechTree techTree;
	public Clan clan;

	void Awake ()
	{
			ResourceManager.SetGameObjectList (this);
	}

	public GameObject GetBuilding (string name)
	{
		for (int i = 0; i < buildings.Length; i++) {
			Building building = buildings [i].GetComponent< Building > ();
			if (building && building.woName == name)
				return buildings [i];
		}
		return null;
	}
	
	public GameObject GetUnit (string name)
	{
		for (int i = 0; i < units.Length; i++) {
			Unit unit = units [i].GetComponent< Unit > ();
			if (unit && unit.woName == name)
				return units [i];
		}
		return null;
	}
	


	public GameObject GetBuilding (int value)
	{
		if (buildings.Length <= value)
			return null;
		Building building = buildings [value].GetComponent< Building > ();
		if (building && building.value == value)
			return building.gameObject;
		return null;
	}
	
	public GameObject GetUnit (int value)
	{
		if (units.Length <= value)
			return null;
		Unit unit = units [value].GetComponent< Unit > ();
		if (unit && unit.value == value) {
			return unit.gameObject;
		}
		return null;
	}
	public TechTree GetTechTree ()
	{
		return techTree;
	}


}
