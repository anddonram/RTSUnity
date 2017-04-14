using UnityEngine;
using System.Collections;

public class Beamer : Tank {

	public override void TakeDamage (int damage)
	{
		base.TakeDamage (damage/2);
	}
}
