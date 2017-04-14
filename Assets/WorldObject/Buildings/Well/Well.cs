using UnityEngine;
using System.Collections;
using RTS;
using UnityEngine.Networking;
public class Well : Building {

	public WaterDeposit water;

	public override void OnStartClient ()
	{
		base.OnStartClient ();
		water.SetPlayer (owner);
	}
	public override void Construct (int amount)
	{
		hitPoints += amount;
		if (hitPoints >= maxHitPoints) {
			hitPoints = maxHitPoints;
			if (UnderConstruction ()) {
				state = WOState.Nothing;

				enabled = true;
				water.gameObject.SetActive(true);
				owner.playerList.Add (water);
				owner.RpcFinishBuilding (gameObject);
			}
		}
	}
	[ServerCallback]
	protected override void OnDestroy ()
	{
		base.OnDestroy ();
		if(owner)
			owner.playerList.Remove (water);
	}
}
