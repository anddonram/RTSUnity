using UnityEngine;
using System.Collections;

public class Gate : Other {
	public Gate gate;
	protected override void Awake ()
	{
		base.Awake ();
		owner = transform.root.GetComponent<Player> ();
		ownerId = owner.netId;
	}
	public void Switch(Unit unit){
		if (IsOpen()) {
			unit.Warp ( gate.transform.position);
			unit.StartMove (gate.transform.position + gate.transform.forward * 2);
		}
	}
	public override void TakeDamage (int damage)
	{
		hitPoints -= damage;
		if (hitPoints <= 0) {
			hitPoints = 0;
			state = RTS.WOState.Destroyed;
		} else if (hitPoints > maxHitPoints) {
			hitPoints = maxHitPoints;
			state = RTS.WOState.Nothing;
		}
	}
	bool IsOpen(){
		return state==RTS.WOState.Nothing&&gate.state==RTS.WOState.Nothing;
	} 
	public override void OnStartClient ()
	{
	}
}
