using UnityEngine;
using System.Collections;
using RTS;
using UnityEngine.Networking;
using System.Collections.Generic;

public class AIHandler :  NetworkBehaviour
{

	public Player player;
	public Terrain terrain;
	public NetworkInstanceId playerId;

	public AIResource airesource;
	public AIBuilding aibuilding;
	public AIAttack aiattack;
	public AITrain aitrain;
	public AITech aitech;

	public BackPropagationAlgorithm bpa;

	public PlayerObjectList playerList;

	[ServerCallback]
	void Awake(){
		bpa = GetComponent<BackPropagationAlgorithm> ();
		if (bpa) {
			bpa.numInput = 7;
			bpa.numHidden = 7;
			bpa.numOutput = 7;
			bpa.Recreate ();
		}
	}

	void Start ()
	{
		playerId = player.netId;
		playerList = player.playerList;
	}


	public int GetMinHarvesters ()
	{
		int pop = player.GetResource (ResourceType.Population);
		int res = 6;
		if (pop <= 4)
			res = 3;
		else if (pop <= 10)
			res = 4;
		else if (pop > 20 && pop <= 35)
			res = 8;
		return res;
	}

	public int GetMinWorkers ()
	{
		int pop = player.GetResource (ResourceType.Population);
		int res = 3;
		if (pop <= 10)
			res = 2;
		else if (pop > 20 && pop <= 35)
			res = 4;
		return res;
	}

	public bool HasResources (WorldObject wo, ResourceType type)
	{
		bool res = player.GetResource (type) >= wo.moneyCost;
		if (type == ResourceType.Water) {
			res = player.GetResource (type) >= wo.waterCost;
		}
		return res;
	}

	public bool HasLimitResources (WorldObject wo, ResourceType type)
	{
		bool res = true;
		if (type == ResourceType.Money) {
			res = player.GetResourceLimit (type) >= wo.moneyCost;
		} else if (type == ResourceType.Water) {
			res = player.GetResourceLimit (type) >= wo.waterCost;
		}
		return res;
	}

	public bool HasLimitResources (Node n, ResourceType type)
	{
		return player.GetResourceLimit (type) >= n.GetCost (type);
	}

	public bool HasResources (Node n, ResourceType type)
	{
		return player.GetResource (type) >= n.GetCost (type);
	}

	public GameObject GetClosestOre ()
	{
		
		OreDeposit res = null;
		float closest = Mathf.Infinity;
		if (playerList.ores.Count > 0) {
	
			foreach (OreDeposit o in playerList.ores) {
				float dist = (transform.position - o.transform.position).sqrMagnitude;
				if (res == null || (dist < closest&&!res.isEmpty())) {
					res = o;
					closest = dist;
				}
			}

		}
		if (!res)
			return null;
		return res.gameObject;
	}

	public GameObject GetClosestWater ()
	{
		WaterDeposit res = null;
		float closest = Mathf.Infinity;
		if (playerList.waters.Count > 0) {

			foreach (WaterDeposit o in playerList.waters) {
				float dist = (transform.position - o.transform.position).sqrMagnitude;
				if (res == null || dist < closest) {
					res = o;
					closest = dist;
				}
			}
		
		}

		if (!res)
			return null;
	
		return res.gameObject;
	}
}
