using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Worker : Unit
{

	public int buildSpeed;
	private float amountBuilt = 0.0f;

	protected override void Update ()
	{
		base.Update ();
		if (nav.velocity.sqrMagnitude == 0 && state == RTS.WOState.Building)
		if (nextBuilding) {
			if (nextBuilding.UnderConstruction () || nextBuilding.hitPoints < nextBuilding.maxHitPoints) {

				amountBuilt += buildSpeed * Time.deltaTime;
				int amount = Mathf.FloorToInt (amountBuilt);
		
				if (amount > 0) {
					amountBuilt -= amount;
			
					nextBuilding.Construct (amount);
					if (!nextBuilding.UnderConstruction () && nextBuilding.hitPoints >= nextBuilding.maxHitPoints) {
						StopActions ();	
					}
				}
			}
		} else
			StopActions ();
	}

	public override void SetBuilding (Building project)
	{
		if (!project.UnderConstruction () && project.hitPoints >= project.maxHitPoints)
			return;
		base.SetBuilding (project);
		if (project.IsOwnedBy (ownerId)) {
			state = RTS.WOState.Building;
		}
	}

	public override void PerformAction (string actionToPerform)
	{
		base.PerformAction (actionToPerform);
		CreateBuilding (actionToPerform);
	}

	public override void StartMove (Vector3 destination)
	{
		base.StartMove (destination);
		amountBuilt = 0.0f;
	}


	private void CreateBuilding (string buildingName)
	{
		Vector3 buildPoint = transform.position + 10 * transform.forward;
		owner.CreateBuilding (buildingName, buildPoint, this);
	}

	public override void OnNetworkDestroy ()
	{
		base.OnNetworkDestroy ();
		if (projector.enabled && owner.IsFindingBuildingLocation ())
			owner.CancelBuilding ();
	}
}
