using UnityEngine;
using System.Collections;
using RTS;
using System.Collections.Generic;
using UnityEngine.Networking;
public abstract class Node : NetworkBehaviour
{
	public Node[] previous;
	public int waterCost, moneyCost;
	public string description;
	public string nodeName;
	public float time;
	public string buildingName;
	[SyncVar]
	public bool developed, developing;

	public bool IsAvailable ()
	{
		bool res = !developed && !developing;
		if (res)
			foreach (Node n in previous) {
				if (!n.developed) {
					res = false;
					break;
				}
			}
		return res;
	}

	public bool IsDeveloped ()
	{
		return developed;
	}

	public void Develop ()
	{
		NetworkServer.Spawn (gameObject);
		developed = true;
		developing = false;
		ApplyEffect ();
	}
	public void SetDeveloping(bool d){
		developing = d;
	}
	protected abstract void ApplyEffect ();
	public int GetCost(ResourceType type){
		if (type == ResourceType.Money)
			return moneyCost;
		else if (type == ResourceType.Water)
			return waterCost;
		else
			return 0;
		
	}
}
