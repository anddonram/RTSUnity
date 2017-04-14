using UnityEngine;
using System.Collections;
using RTS;
using UnityEngine.Networking;
using System.Collections.Generic;

public class AIResource : NetworkBehaviour
{
	public AIHandler aihandler;
	List<Harvester> nothing = new List<Harvester> ();
	List<Harvester> water = new List<Harvester> ();
	List<Harvester> ore = new List<Harvester> ();
	[ServerCallback]
	void Start ()
	{
		InvokeRepeating ("Recalculate", 1+Random.value*3, 10);
	}

	private bool clear;
	private int moneyRequest;
	private int waterRequest;

	[Server]
	void Recalculate ()
	{
		ore.Clear ();
		water.Clear ();
		nothing.Clear ();
		foreach (Harvester h in aihandler.playerList.harvesters) {
			if (h.state == WOState.Harvesting || h.state == WOState.Emptying) {
				if (h.GetResourceType () == ResourceType.Money)
					ore.Add (h);
				else if (h.GetResourceType () == ResourceType.Water) {
					water.Add (h);
				} 
			} else if (h.state == WOState.Nothing) {
				nothing.Add (h);
			}
		}

		float waterRatio = waterRequest * 1f / (waterRequest + moneyRequest);
		int expectedWater = (int)(aihandler.playerList.harvesters.Count * waterRatio);

		int expectedMoney = aihandler.playerList.harvesters.Count - expectedWater; 
		GameObject oreDep = aihandler.GetClosestOre ();
		GameObject waterDep = aihandler.GetClosestWater ();

		foreach (Harvester h in nothing) {
			h.MouseClick (oreDep, oreDep.transform.position, aihandler.playerId, false);
		}

		ore.AddRange (nothing);
		if (expectedWater > water.Count) {
			int allocate = expectedWater - water.Count;	
			for (int i = 0; i < allocate&&i<ore.Count; i++) {
				ore [i].MouseClick (waterDep, waterDep.transform.position, aihandler.playerId, false);

			}
		} else if (expectedMoney > ore.Count) {
			int allocate = expectedMoney - ore.Count;	
			for (int i = 0; i < allocate&&i<water.Count; i++) {
				water [i].MouseClick (oreDep, oreDep.transform.position, aihandler.playerId, false);
			}
		}
		if (clear) {
			clear = !clear;
			moneyRequest = 1;
			waterRequest = 1;
		}
	}
	public Vector3 GetNeededResourcePosition(){
		Vector3 r;
		if (moneyRequest > waterRequest) {
			r = aihandler.GetClosestOre ().transform.position;
		} else if (moneyRequest < waterRequest) {
			r = aihandler.GetClosestWater ().transform.position;
		} else
			r = transform.position;
		return r;
	}
	public void AllocateMoney (int amount)
	{
		moneyRequest+=amount;
	}

	public void AllocateWater (int amount)
	{
		waterRequest+=amount;
	}
}
