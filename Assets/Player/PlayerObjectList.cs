using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using RTS;
public class PlayerObjectList : NetworkBehaviour
{

	public HashSet<Unit> units;
	public HashSet<BaseUnit> baseUnits;
	public HashSet<Worker> workers;
	public HashSet<Harvester> harvesters;

	public HashSet<Building> buildings;
	public HashSet<TrainingField> fields;
	public HashSet<Hut> huts;

	public HashSet<OreDeposit> ores;
	public HashSet<WaterDeposit> waters;

	void Awake ()
	{
		units = new HashSet<Unit> ();
		baseUnits = new HashSet<BaseUnit> ();
		workers = new HashSet<Worker> ();
		harvesters = new HashSet<Harvester> ();
		buildings = new HashSet<Building> ();
		fields = new HashSet<TrainingField> ();
		huts = new HashSet<Hut> ();
		ores = new HashSet<OreDeposit> ();
		waters = new HashSet<WaterDeposit> ();
	}
	void Start(){
		ores.UnionWith (FindObjectsOfType<OreDeposit> ());
		waters.UnionWith (FindObjectsOfType<WaterDeposit> ());
	}

	public void Add (WorldObject wo)
	{
		AddToList (wo);
		RpcAdd (wo.netId);
	}
	[ClientRpc]
	void RpcAdd(NetworkInstanceId id){
		AddToList (ClientScene.FindLocalObject(netId).GetComponent<WorldObject>());
	}
	void AddToList (WorldObject wo)
	{
		if (wo is Unit) {
			units.Add (wo as Unit);
			if (wo is BaseUnit) {
				baseUnits.Add (wo as BaseUnit);
				if (wo is Harvester)
					harvesters.Add (wo as Harvester);
			} else if (wo is Worker)
				workers.Add (wo as Worker);
		} else if (wo is Building) {
			buildings.Add (wo as Building);
			if (wo is Hut) {
				huts.Add (wo as Hut);
			} else if (wo is TrainingField) {
				fields.Add (wo as TrainingField);
			} 
		} else if (wo is WaterDeposit) {
			waters.Add (wo as WaterDeposit);
		}
	}

	public void Remove (WorldObject wo)
	{
		RemoveFromList (wo);
		RpcRemove (wo.netId);

	}
	[ClientRpc]
	void RpcRemove(NetworkInstanceId id){
		RemoveFromList (ClientScene.FindLocalObject(netId).GetComponent<WorldObject>());
	}
	void RemoveFromList (WorldObject wo)
	{
		if (wo is Unit) {
			units.Remove (wo as Unit);
			if (wo is BaseUnit) {
				baseUnits.Remove (wo as BaseUnit);
				if (wo is Harvester)
					harvesters.Remove (wo as Harvester);
			} else if (wo is Worker)
				workers.Remove (wo as Worker);
		} else if (wo is Building) {
			buildings.Remove (wo as Building);
			if (wo is Hut) {
				huts.Remove (wo as Hut);
			} else if (wo is TrainingField) {
				fields.Remove (wo as TrainingField);
			}
		} else if (wo is WaterDeposit) {
			waters.Remove (wo as WaterDeposit);
		}
	}
}
