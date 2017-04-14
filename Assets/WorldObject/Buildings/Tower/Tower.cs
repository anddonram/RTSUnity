using UnityEngine;
using System.Collections;
using RTS;
using UnityEngine.Networking;
public class Tower : Building {

	private Unit onTop;
	[Server]
	public override void Enter(Unit u){
		base.Enter (u);
		if (!onTop) {
			onTop = u;
			onTop.state=WOState.Entering;
			Activate (false);
			onTop.transform.position = transform.position + Vector3.up * 4;
			onTop.prox.visRange+=prox.visRange;
			onTop.weaponRange +=prox.visRange;
		} else
			onTop.StopActions();
	}
	[Server]
	public override void Exit(){	
		base.Exit ();
		if (onTop) {
			onTop.transform.position = spawnPoint.position;
			Activate (true);

			onTop.StopActions();
			onTop.StartMove(rallyPoint);
			onTop.prox.visRange-=prox.visRange;
			onTop.weaponRange -=prox.visRange;
			onTop = null;
		
		}
	}
	void Activate(bool active){
		onTop.GetComponent<NavMeshAgent> ().enabled=active;
		owner.RpcActivate (onTop.gameObject,active);

	}
	public override bool CanAttackOnRange ()
	{
		return onTop&&onTop.CanAttackOnRange ();
	}
	public override void BeginAttack (WorldObject wo)
	{
		onTop.BeginAttack (wo);
	}

	protected override void OnDisable ()
	{
		base.OnDisable ();
		if (isServer) {
			Exit ();
		}
	}
}
