using UnityEngine;
using System.Collections;
using RTS;
using UnityEngine.Networking;
using System.Collections.Generic;

public class AIBuilding : NetworkBehaviour
{
	public AIHandler aihandler;

	private Queue<string> queue;
	private AIResource airesource;
	private HashSet<int> buildingTypes;

	[ServerCallback]
	void Start ()
	{
		buildingTypes = new HashSet<int> ();
		queue = new Queue<string> ();
		airesource = aihandler.airesource;
		InvokeRepeating ("Recalculate", 3 + Random.value * 3, 4);
	}

	[Server]
	void Recalculate ()
	{
		buildingTypes.Clear ();
		foreach (TrainingField t in aihandler.playerList.fields) {
			buildingTypes.Add (t.value);
		}

		float hutsChance = 1f / (2 << aihandler.playerList.huts.Count);
		float buildChance = 1f / (2 << aihandler.player.buildings.childCount);
		if (hutsChance > Random.value) {
			BuildHut ();
		}
		if (buildChance > Random.value) {
			string woName = aihandler.player.GetBuilding ((int)(Random.value * 8)).GetComponent<WorldObject> ().woName;
			if (woName != "Specializer")
				Build (woName);
		}
		ChooseBuilding ();

	}

	void ChooseBuilding ()
	{
		if (aihandler.playerList.workers.Count == 0)
			return;
		foreach (Worker w in aihandler.playerList.workers) {
			if (queue.Count == 0)
				break;
			if (w.state == WOState.Nothing) {
				WorldObject wo = aihandler.player.GetBuilding (queue.Peek ()).GetComponent<WorldObject> ();
				if (!aihandler.HasResources (wo, ResourceType.Money)) {
					airesource.AllocateMoney (wo.moneyCost);
					if (!aihandler.HasLimitResources (wo, ResourceType.Money)) {
						BuildHut ();
					}
				} 
				if (!aihandler.HasResources (wo, ResourceType.Water)) {
					airesource.AllocateWater (wo.waterCost);
					if (!aihandler.HasLimitResources (wo, ResourceType.Water)) {
						BuildHut ();
					}
				} else if (aihandler.HasResources (wo, ResourceType.Money)) {
					w.getOwner ().CreateBuilding (queue.Peek (), Vector3.zero, w);

					Vector3	playerPos = Vector3.zero;
					int tries = 1;
				
					Vector3 place;
					if (queue.Peek () == "Hut") {
						place = airesource.GetNeededResourcePosition ();
					} else {
						place = transform.position;
					}
					do {	
						playerPos = aihandler.player.FindRandomPlace (tries, place);
						tries++;
					} while(!w.getOwner ().CanPlaceBuilding () && tries < 10);

					w.getOwner ().CancelBuilding ();
					if (tries < 10) {
						w.getOwner ().Construction (queue.Dequeue (), playerPos, w.gameObject);
						buildingTypes.Add (wo.value);
					} else {
						queue.Enqueue (queue.Dequeue ());
					}
				}
			}
		}
	}

	public void Build (string buildName)
	{
		if (!queue.Contains (buildName)) {
			queue.Enqueue (buildName);
		}
	}

	public void BuildTrainingField ()
	{
		
		if (!buildingTypes.Contains (0)) {
			Build ("WarFactory");
		} else if (!buildingTypes.Contains (1)) {
			Build ("Refinery");
		} else if (!buildingTypes.Contains (2)) {
			Build ("Chemistry");
		} else if (buildingTypes.Contains (0) && buildingTypes.Contains (1) && buildingTypes.Contains (2)) {
			Build ("Specializer");
		}
	
	}

	public void BuildHut ()
	{
		Build ("Hut");
	}

	public void AskForTrainingFields (int value)
	{
		if ((value & 1) != 0)
			Build ("WarFactory");
		if ((value & 2) != 0)
			Build ("Refinery");
		if ((value & 4) != 0)
			Build ("Chemistry");
		
			
	}
}
