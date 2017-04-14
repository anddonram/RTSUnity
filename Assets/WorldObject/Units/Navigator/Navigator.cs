using UnityEngine;
using System.Collections;

public class Navigator : BaseUnit {
	public override bool CanAttackClose ()
	{
		return true;
	}
	protected override void CloseCombat ()
	{
		base.CloseCombat ();
		target.TakeDamage (2);
	}
}
