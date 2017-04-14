using UnityEngine;
using System.Collections;
using RTS;

public class Hero : Unit
{
	private bool activated = false;
	public int attack;

	public override bool CanAttackClose(){return true;}

	public override void SpecialMove ()
	{
		activated = !activated;
	}
	protected override void CloseCombat ()
	{
		base.CloseCombat ();
		int damage = attack;
		if (activated && GetStamina() > 40) {
			damage += 50;
			ConsumeStamina(40);
		}	
		target.TakeDamage (damage);
	}
	public override string[] GetInfo ()
	{
		string[] info=base.GetInfo ();

		string[] res=new string[info.Length+1];
		info.CopyTo (res,0); 
		res[info.Length] = "activated" + activated;
		return res;
	}
}
