using UnityEngine;
using System.Collections;

public class Cannon : Projectile {
	private Vector3 p0;
	private Vector3 p1;
	private Vector3 p2;
	private float t;
	private float dist;
	protected override void MoveProjectile(){
		
		t+=Time.deltaTime/(velocity+0.1f*dist);

		float unomenost=1-t;
		Vector3 point = unomenost * unomenost * p0 + 2 *unomenost*t * p1 + t * t *p2;
		rb.position = point+deltaPos;
	}
	public override void SetTarget (WorldObject target)
	{
		base.SetTarget (target);
		t = 0;
		p0 = transform.position;
		p2 = target.transform.position;
		p1 = (p0 +p2) / 2;
		dist = Vector3.Distance (p0, p2);
		p1.y += Mathf.Abs(dist);
		}
}
