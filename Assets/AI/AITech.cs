using UnityEngine;
using System.Collections;
using RTS;
using UnityEngine.Networking;
using System.Collections.Generic;
public class AITech : NetworkBehaviour {
	public AIHandler aihandler;
	private AIBuilding aibuilding;
	private AIResource airesource;
	private TechTree tree;

	[ServerCallback]
	void Start () {
		tree = aihandler.player.techTree;

		aibuilding = aihandler.aibuilding;
		airesource = aihandler.airesource;
		InvokeRepeating ("Recalculate",7+Random.value*3,10);
	}
	
	[Server]
	void Recalculate () {
		Node n=ChooseTech ();
		if(n)
			Develop(n);		
	}
	Node ChooseTech(){
		Node n = null;
		foreach(Node node in tree.GetNodes()){
			if (node.IsAvailable ()) {
				n = node;
				break;
			}
		}
		return n;
	}
	void Develop(Node n){
		Building b=BuildingAvailable (n.buildingName);

		if (b) {
			if (!aihandler.HasResources (n, ResourceType.Money)) {
				airesource.AllocateMoney (n.GetCost (ResourceType.Money));
				if (!aihandler.HasLimitResources (n, ResourceType.Money)) {
					aibuilding.BuildHut ();
				}
			} 
			if (!aihandler.HasResources (n, ResourceType.Water)) {
				airesource.AllocateWater (n.GetCost (ResourceType.Water));
				if (!aihandler.HasLimitResources (n, ResourceType.Water)) {
					aibuilding.BuildHut ();
				}
			} else if (aihandler.HasResources (n, ResourceType.Money)) {
				b.StartNode (n);
			
			}
		} else {
			BuildRequiredBuilding (n.buildingName);
		
		}
	}
	Building BuildingAvailable(string bName){
		HashSet<Building> buildings = aihandler.playerList.buildings;
		Building res = null;
		foreach(Building b in buildings){
			if (b.woName == bName) {
				res = b;
				break;
			}
		}
			return res;
		}
	void BuildRequiredBuilding (string bName){
		aibuilding.Build(bName);
	}
}
