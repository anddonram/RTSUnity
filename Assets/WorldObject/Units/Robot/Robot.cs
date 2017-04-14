using UnityEngine;
using System.Collections;
using RTS;
public class Robot : BaseUnit {
	public int attack;


	public override bool CanAttackClose(){return true;}

	protected override void CloseCombat () {
		base.CloseCombat ();
		target.TakeDamage (attack);

	}


}
