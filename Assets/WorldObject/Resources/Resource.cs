using UnityEngine;
using System.Collections;
using RTS;
using UnityEngine.Networking;
public class Resource : WorldObject
{

	protected ResourceType type;

	protected override void Awake ()
	{
		base.Awake ();
		owner = transform.root.GetComponent<Player> ();
		ownerId = owner.netId;
	}
		

	public virtual void Remove (int amount)
	{
		hitPoints -= amount;
		if (hitPoints < 0)
			hitPoints = 0;
		if (hitPoints > maxHitPoints)
			hitPoints = maxHitPoints;
	}

	public bool isEmpty ()
	{
		return hitPoints <= 0;
	}

	public ResourceType GetResourceType ()
	{
		return type;
	}

	public override void TakeDamage (int damage)
	{
	}

	public override void OnStartClient ()
	{
	}
}
