using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
public class WaterDeposit : Resource {
	[ServerCallback]
	protected override void Start(){
		base.Start ();
		type = RTS.ResourceType.Water;
		maxHitPoints = 100;
		hitPoints =100;
	}
	public override void Remove(int amount){}
}
