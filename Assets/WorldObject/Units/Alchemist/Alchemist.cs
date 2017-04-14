using UnityEngine;
using System.Collections;
using RTS;
using UnityEngine.Networking;
using System.Collections.Generic;
public class Alchemist : BaseUnit
{
	[SyncVar]
	public int
		capacity;
	public float collectionAmount, depositAmount;
	private float currentDeposit = 0.0f;
	[SyncVar]
	private float
		currentLoad = 0;
	private WaterDeposit resourceDeposit;
	private OreDeposit resourceStore;

	protected override void Update ()
	{
		base.Update ();
		if (nav.velocity.sqrMagnitude == 0)
			switch (state) {
			case WOState.Harvesting:
				if (currentLoad >= capacity) {
					currentLoad = Mathf.Floor (currentLoad);	
					state = WOState.Emptying;		
					StartMove (resourceStore.transform.position);

				} else {
					Collect ();
				}
				break;
			case WOState.Emptying:
					
				Deposit ();
				if (currentLoad <= 0) {
				StartMove (resourceDeposit.transform.position);
				state = WOState.Harvesting;
				}
				break;
			}
	}
	public override void SpecialMove ()
	{
		base.SpecialMove ();
		if (GetStamina () > 95) {
			ConsumeStamina(95);
			owner.RpcRain ();
		}
	}
	protected override void OnWorldObject (WorldObject wo)
	{
		base.OnWorldObject (wo);
		if (wo is OreDeposit) {
			StartHarvest (wo as OreDeposit);
		}
	}
	
	private void StartHarvest (OreDeposit resource)
	{
		resourceStore = resource;
		GetClosestStore ();
		StartMove (resourceDeposit.transform.position);
		state = WOState.Harvesting;
	}

	public override void StopActions ()
	{
		base.StopActions ();
		resourceDeposit = null;
		resourceStore = null;
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
		if (deposit >= 2) {
			if (deposit > currentLoad)
				deposit = Mathf.FloorToInt (currentLoad);
			currentDeposit -= deposit;
			currentLoad -= deposit;
			resourceStore.Remove (-deposit >> 2);
		}
	}

	public override float GetActionPoints ()
	{
		return currentLoad / capacity;
	}

	private void GetClosestStore ()
	{
		HashSet<WaterDeposit> water = owner.playerList.waters;

		Vector3 pos = resourceStore.transform.position;
		Vector3 current = ResourceManager.InvalidPosition;
		foreach (WaterDeposit h in water) {
			Vector3 distance = h.transform.position - pos;
			if (distance.sqrMagnitude < current.sqrMagnitude) {
				resourceDeposit = h;
				current = distance;
			}
		}
	}
	public override bool CanAttackClose ()
	{
		return true;
	}
	protected override void CloseCombat ()
	{
		base.CloseCombat ();
		target.TakeDamage ((int)(capacity-currentLoad)/2);
	}
}
