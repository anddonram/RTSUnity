using RTS;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class Unit : WorldObject
{

	protected NavMeshAgent nav;
	protected Building nextBuilding;
	[SyncVar]
	private float
		stamina = 100;
	public float staminaSpeed = 5;
	[SyncVar]
	public bool
		specialActivated = false;
	public string specialMoveName;

	protected override void Awake ()
	{
		base.Awake ();
		nav = GetComponent<NavMeshAgent> ();
	
	}

	[ServerCallback]
	protected override void Update ()
	{

		base.Update ();
		if (nav.velocity.sqrMagnitude == 0) {
			ConsumeStamina (-staminaSpeed * Time.deltaTime);
		} else if (running) {
			ConsumeStamina (staminaSpeed * Time.deltaTime);
			if (stamina == 0)
				Run (false);
		} 
		if (WorkManager.VectorEquals (transform.position, nav.destination, .5f))
			switch (state) {
			case WOState.Gating:
				(target as Gate).Switch (this);
				break;
			case WOState.Waiting:
				if (!nextBuilding.UnderConstruction ()) {
					nextBuilding.Enter (this);
				}
				break;
			case WOState.Rocking:
				(target as Rock).Push (this);
				break;
			}

	}

	public virtual void SpecialMove ()
	{

	}

	protected override void OnGround (Vector3 hitPoint)
	{
		base.OnGround (hitPoint);
		StartMove (hitPoint + transform.position.y * Vector3.up);
		StopActions ();

	}
	//Be careful with resources without owner!!
	protected override void OnWorldObject (WorldObject wo)
	{
		base.OnWorldObject (wo);
		if (wo.IsOwnedBy (ownerId)) {
			if (wo is Building) {
				SetBuilding (wo as Building);
			}
		} else if (wo is Gate) {
			state = WOState.Gating;
			target = wo;
			StartMove (wo.transform.position);
		} else if (wo is Rock) {
			state = WOState.Rocking;
			target = wo;
			StartMove (wo.transform.position);
		}
	}

	public virtual void StartMove (Vector3 destination)
	{
		if(nav.enabled&&nav.isOnNavMesh)
			nav.destination = destination;
	}

	public virtual void StopActions ()
	{
		nextBuilding = null;
		state = WOState.Nothing;
	}

	public void Warp (Vector3 pos)
	{
		nav.Warp (pos);
		state = WOState.Nothing;
	}

	public virtual void SetBuilding (Building building)
	{
		nextBuilding = building;
		StartMove (building.spawnPoint.position);
		state = WOState.Waiting;
	}

	public override string[] GetInfo ()
	{
		List<string> s = new List<string> ();

		if (CanAttack ()) {
			s.Add ("range: " + weaponRange);
			s.Add ("cadency: " + weaponRechargeTime);
		}
		if (specialActivated) {
			s.Add ("special: " + specialMoveName);
		}
		return s.ToArray ();
	}

	public override Vector3 GetFlagPosition ()
	{
		Vector3 res = ResourceManager.InvalidPosition;
		if (nav.enabled&&!WorkManager.VectorEquals (nav.destination, transform.position, 1))
			res=nav.destination;
		return res ;
	}

	public virtual bool CanRide ()
	{
		return true;
	}
		

	public override void OnStartClient ()
	{
		base.OnStartClient ();
		transform.SetParent (owner.units);
	}


	protected override void OnDisable ()
	{
		base.OnDisable ();
		if (isClient&&owner && owner.isLocalPlayer) {
			owner.SelectionUnits.Remove (this);
		}
	}

	[ServerCallback]
	protected override void OnDestroy ()
	{
		base.OnDestroy ();
		owner.AddResource (ResourceType.Population, -1);
	}

	public void ConsumeStamina (float amount)
	{
		stamina -= amount;
		if (stamina < 0)
			stamina = 0;
		else if (stamina > 100)
			stamina = 100;
	}

	public override void Run (bool run)
	{
		if (!run&&running)
			nav.speed *= 0.5f;
		else if ( run && !running )
			nav.speed *= 2;

		base.Run (run);

	}

	public float GetStamina ()
	{
		return stamina;
	}

	public Vector3 GetDestination ()
	{
		return nav.destination;
	}

	public Vector3 GetVelocity ()
	{
		return nav.velocity;
	}


}
