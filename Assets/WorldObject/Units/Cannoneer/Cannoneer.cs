using UnityEngine;
using System.Collections;
using RTS;
using UnityEngine.Networking;
public class Cannoneer : BaseUnit {
	public Projectile projectile;
	private Projectile current;
	[ServerCallback]
	protected override void Start ()
	{
		base.Start ();
		current = Instantiate (projectile).GetComponent<Projectile>();
		current.gameObject.SetActive (false);
	}
	protected override void UseRangeWeapon () {
		base.UseRangeWeapon();
		GameObject projectileObject = current.gameObject;
		if (!projectileObject.activeSelf) {
			Vector3 spawnPoint = transform.position;
			spawnPoint.x += (2.1f * transform.forward.x);
			spawnPoint.y += 1.4f;
			spawnPoint.z += (2.1f * transform.forward.z);

			projectileObject.transform.position = spawnPoint;
			projectileObject.transform.rotation = transform.rotation;
			current.gameObject.SetActive (true);
			current.SetTarget (target);

			NetworkServer.Spawn (current.gameObject);
		}
	}
	[ServerCallback]
	protected override void OnDestroy ()
	{
		base.OnDestroy ();
		if (current)
		if (current.gameObject.activeSelf) {
			NetworkServer.Destroy (current.gameObject);
		} else {
			Destroy (current.gameObject);
		}
	}
}
