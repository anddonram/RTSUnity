using UnityEngine;
using System.Collections;
using RTS;
using UnityEngine.Networking;
using System.Collections.Generic;

public class Harvester : BaseUnit
{
	[SyncVar]
	public int
		capacity;
	public float collectionAmount, depositAmount;
	private float currentDeposit = 0.0f;
	[SyncVar]
	private float
		currentLoad = 0;
	private ResourceType type;
	private Resource resourceDeposit;

	protected override void Update ()
	{
		base.Update ();	
		if (nav.velocity.sqrMagnitude == 0)
			switch (state) {
			case WOState.Harvesting:
				if (currentLoad >= capacity) {
					currentLoad = Mathf.Floor (currentLoad);	
					GetClosestStore ();
					if (nextBuilding) {
						state = WOState.Emptying;		
						StartMove (nextBuilding.transform.position);
					}
				} else if (!resourceDeposit.isEmpty ())
					Collect ();

				break;
			case WOState.Emptying:

				if (owner.GetResource (type) < owner.GetResourceLimit (type)) {
					Deposit ();
					if (currentLoad <= 0) {
						if (resourceDeposit && !resourceDeposit.isEmpty ()) {
							StartMove (resourceDeposit.transform.position);
							state = WOState.Harvesting;
						} else {
							StopActions ();
						}
					}
				}
				break;
			}
			
	}

	protected override void OnWorldObject (WorldObject wo)
	{
		base.OnWorldObject (wo);
		if (wo is Resource) {
			Resource res = wo as Resource;
			if (!res.isEmpty ()) {
				StartHarvest (res);
			} 
		} else if (currentLoad > 0 && wo is Hut) {
			StartMove (wo.transform.position);
			state = WOState.Emptying;
		} else if (wo is Well) {
			Well well = wo as Well;
			if (!well.UnderConstruction ())
				StartHarvest (well.water);	
		}
	}

	private void StartHarvest (Resource resource)
	{
		resourceDeposit = resource;
		StartMove (resource.transform.position);
		if (type != resource.GetResourceType ()) {
			type = resource.GetResourceType ();
			currentLoad = 0;
		}
		state = WOState.Harvesting;
	}

	private void Collect ()
	{
		currentLoad += collectionAmount * Time.deltaTime;
		currentDeposit += collectionAmount * Time.deltaTime;
		int deposit = Mathf.FloorToInt (currentDeposit);
		if (deposit >= 1) {
			currentDeposit -= deposit;
			resourceDeposit.Remove (deposit);
		}
	}

	private void Deposit ()
	{
		currentDeposit += depositAmount * Time.deltaTime;
		int deposit = Mathf.FloorToInt (currentDeposit);
		if (deposit >= 1) {
			if (deposit > currentLoad)
				deposit = Mathf.FloorToInt (currentLoad);
			currentDeposit -= deposit;
			currentLoad -= deposit;
			owner.AddResource (type, deposit);
		}
	}

	public override float GetActionPoints ()
	{
		return currentLoad / capacity;
	}

	private void GetClosestStore ()
	{
		HashSet<Hut> huts = owner.playerList.huts;
		if (huts.Count == 0) {
			return;
		} else {
			Vector3 pos = transform.position;
			Vector3 current = ResourceManager.InvalidPosition;
			foreach (Hut h in huts) {
				if (h.UnderConstruction ())
					continue;
				Vector3 distance = h.transform.position - pos;
				if (distance.sqrMagnitude < current.sqrMagnitude) {
					nextBuilding = h;
					current = distance;
				}
			}
		}
	}

	public ResourceType GetResourceType ()
	{
		return type;
	}
}
