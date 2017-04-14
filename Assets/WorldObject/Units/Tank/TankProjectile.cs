using UnityEngine;
using System.Collections;

public class TankProjectile : Projectile {
	private Vector3 p0;

	private Vector3 p2;
	private float t;
	protected override void MoveProjectile(){

		t+=Time.deltaTime/(velocity+0.1f);
		float unomenost=1-t;
		Vector3 point = unomenost  * p0 + t *p2;
		rb.position = point+deltaPos+Vector3.up;
	}
	public override void SetTarget (WorldObject target)
	{
		base.SetTarget (target);
		t = 0;
		p0 = transform.position;
		p2 = target.transform.position;

	}
}
