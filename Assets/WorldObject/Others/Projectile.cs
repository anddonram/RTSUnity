using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Projectile : NetworkBehaviour
{
	public Rigidbody rb;

	public float velocity = 1;
	public int damage = 1;
	private bool detectedMovement;
	protected WorldObject target;
	protected Vector3 deltaPos;

	[ServerCallback]
	protected virtual void Update ()
	{
		if (!target) {
			GroundImpact ();
			return;
		}
		if (!detectedMovement) {
			deltaPos = target.transform.position - deltaPos;
			detectedMovement = true;
		}
		MoveProjectile ();
	}

	public virtual void SetTarget (WorldObject target)
	{
		this.target = target;
		deltaPos = target.transform.position;
		detectedMovement = false;
	}

	[ServerCallback]
	public void OnTriggerEnter (Collider other)
	{
		if (other.CompareTag ("Ground")) {
			GroundImpact ();
			return;
		}
		WorldObject wo = other.transform.parent.GetComponent<WorldObject> ();
		if (!wo) {
			return;
		}
		if (wo == target) {
			InflictDamage (wo);
			NetworkServer.UnSpawn (gameObject);
			gameObject.SetActive (false);
		} else
			wo.TakeDamage (2);
	}

	protected virtual void MoveProjectile ()
	{
		float positionChange = Time.deltaTime * velocity;
		rb.position += (positionChange * transform.forward) + deltaPos- Vector3.up * Time.deltaTime * Time.deltaTime;
	}

	protected virtual void GroundImpact ()
	{
		NetworkServer.UnSpawn (gameObject);
		gameObject.SetActive (false);
	}

	protected virtual void InflictDamage (WorldObject wo)
	{
		wo.TakeDamage (damage);
	}
}
