using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;
using UnityEngine.Networking;


public class WorldObject : NetworkBehaviour
{
	/*
	 * valores del objeto
	*/
	public string woName;
	public int value;

	public int moneyCost, waterCost;
	/** vida del objeto
	 */
	[SyncVar]
	public int
		hitPoints, maxHitPoints = 100;
	[SyncVar]
	protected NetworkInstanceId
		ownerId;
	protected Player owner;

	public Player getOwner ()
	{
		return owner;
	}

	public string[] actions = { };

	public virtual string[] GetActions ()
	{
		return actions;
	}

	public WOState state = WOState.Nothing;
	protected bool running = false;
	public NetworkProximityChecker prox;
	public Projector projector, lineOfSight;

	public void SetSelection (bool selected)
	{
		if (projector)
			projector.enabled = selected;
	}

	protected virtual void Awake ()
	{
		hitPoints = maxHitPoints;

	}

	[ServerCallback]
	protected virtual void Start ()
	{
	}


	[ServerCallback]
	protected virtual void Update ()
	{
		currentWeaponChargeTime += Time.deltaTime;
		switch (state) {
		case WOState.Attacking:
			if (!target) {
				targetInContact = false;
				state = WOState.Nothing;
				return;
			}
			int distance = TargetInRange ();
		
			if (distance == 1 || (distance == -1 && !CanAttackClose ()))
				AdjustPosition ();
			else {
				
				if (!TargetInFrontOfWeapon ()) {
					AimAtTarget ();
				} else if (ReadyToFire ()) {					
					UseWeapon (distance);
				}
			}
			break;
		}	

	}

	public void MouseClick (GameObject hitObject, Vector3 hitPoint,
	                        NetworkInstanceId id, bool run)
	{
		if (hitObject && IsOwnedBy (id)) {
			Run (run);
			if (hitObject.CompareTag ("Ground")) {
				OnGround (hitPoint);
			} else {
				OnWorldObject (hitObject.GetComponent<WorldObject> ());
			}
		}
	}

	public virtual void PerformAction (string actionToPerform)
	{
	}

	public virtual void Run (bool run)
	{
		running = run;
	}

	protected virtual void OnGround (Vector3 hitPoint)
	{
		target = null;
		owner.RpcSetFlag (hitPoint);
	}

	protected virtual void OnWorldObject (WorldObject wo)
	{
		if (CanAttack () && !wo.IsOwnedBy (ownerId)) {
			BeginAttack (wo);
		}
	}

	public bool IsOwnedBy (NetworkInstanceId id)
	{
		return ownerId.Value == id.Value;
	}

	public void SetPlayer (Player p)
	{
		ownerId = p.netId;
		owner = p;
		SetTeamColor ();
	}

	protected void SetTeamColor ()
	{
		TeamColor[] teamColors = GetComponentsInChildren< TeamColor > ();
		foreach (TeamColor teamColor in teamColors)
			teamColor.GetComponent<Renderer> ().material.color = owner.teamColor;
	}


	protected void Consume (string name)
	{
		GameObject g = owner.GetUnit (name);
		if (!g) {
			g = owner.GetBuilding (name);
		}
		WorldObject wo = g.GetComponent<WorldObject> ();
		owner.AddResource (ResourceType.Money, -wo.moneyCost);
		owner.AddResource (ResourceType.Water, -wo.waterCost);
	}

	protected void RecoverResources (int value)
	{
		GameObject g = owner.GetUnit (value);
		if (!g) {
			g = owner.GetBuilding (value);
		}
		WorldObject wo = g.GetComponent<WorldObject> ();
		owner.AddResource (ResourceType.Money, wo.moneyCost);
		owner.AddResource (ResourceType.Water, wo.waterCost);
	}

	public float weaponRange = 10.0f;
	public float weaponMinRange = 4f;
	public float weaponRechargeTime = 1.0f;
	private float currentWeaponChargeTime = 0;
	protected WorldObject target = null;
	public bool targetInContact = false;

	public bool CanAttack ()
	{
		return CanAttackOnRange () || CanAttackClose ();
	}

	public virtual bool CanAttackOnRange ()
	{
		return weaponRange >= 4;
	}

	public virtual bool CanAttackClose ()
	{
		return false;
	}

	public virtual void BeginAttack (WorldObject target)
	{
		this.target = target;
		state = WOState.Attacking;
	}

	private int TargetInRange ()
	{
		Vector3 targetLocation = target.transform.position;
		Vector3 direction = targetLocation - transform.position;
		float targetDistance = direction.sqrMagnitude;
		if (targetDistance < weaponMinRange * weaponMinRange || targetInContact)
			return -1;
		else if (targetDistance > weaponRange * weaponRange)
			return 1;
		return 0;
	}

	private void AdjustPosition ()
	{
		Unit self = this as Unit;
		if (self) {
			self.StartMove (FindNearestAttackPosition ());
		} else
			state = WOState.Nothing;	
	}

	private Vector3 FindNearestAttackPosition ()
	{

		Vector3 targetLocation = target.transform.position;
		if (CanAttackClose ()) {
			if (targetInContact)
				return transform.position;
			else
				return targetLocation;
		}

		Vector3 direction = targetLocation - transform.position;
		float targetDistance = direction.magnitude;
		float distanceToTravel;
		if (targetDistance > weaponRange) {
			distanceToTravel = targetDistance - (0.9f * weaponRange);
			return Vector3.Lerp (transform.position, targetLocation, distanceToTravel / targetDistance);
		}
		distanceToTravel = (1.1f * weaponMinRange) - targetDistance;

		return  transform.position - distanceToTravel * direction.normalized;
	}

	bool TargetInFrontOfWeapon ()
	{
		Vector3 direction = (target.transform.position - transform.position);
		direction.y = 0;
		direction.Normalize ();
	
		bool res = this is Building || WorkManager.VectorEquals (direction, transform.forward, 0.02f) || direction == Vector3.zero;
	
		return res;
	}

	void AimAtTarget ()
	{
		transform.rotation = Quaternion.RotateTowards (transform.rotation,
			Quaternion.LookRotation (target.transform.position - transform.position), 3);
	}

	void UseWeapon (int distance)
	{
		currentWeaponChargeTime = 0.0f;
		if (CanAttackOnRange () && distance == 0)
			UseRangeWeapon ();
		else if (CanAttackClose ())
			CloseCombat ();
	}

	protected virtual void UseRangeWeapon ()
	{
		if (target.CanAttack () && target.state != WOState.Attacking) {
			target.BeginAttack (this);
		}
	}


	protected virtual void CloseCombat ()
	{
		if (target.CanAttack ()) {
			target.BeginAttack (this);
			target.Run (true);
		}
	}

	private bool ReadyToFire ()
	{
		return currentWeaponChargeTime >= weaponRechargeTime;
	}

	public virtual void TakeDamage (int damage)
	{
		hitPoints -= damage;
		if (hitPoints <= 0)
			NetworkServer.Destroy (gameObject);
		else if (hitPoints > maxHitPoints)
			hitPoints = maxHitPoints;
	}

	public override void OnStartClient ()
	{
		if (!owner)
			owner = ClientScene.FindLocalObject (ownerId).GetComponent<Player> ();
		SetTeamColor ();

		if (lineOfSight && (owner.isLocalPlayer || Player.GetLocalPlayer ().spectator)) {
			lineOfSight.enabled = true;
			lineOfSight.orthographicSize = prox.visRange;
			lineOfSight.transform.position += Vector3.up * 20;
		}
	}


	protected virtual void OnDisable ()
	{
	}


	public virtual string[] GetInfo ()
	{
		return new string[0];
	}

	public virtual Vector3 GetFlagPosition ()
	{
		return ResourceManager.InvalidPosition;
	}

	public virtual float GetActionPoints ()
	{
		return 0;
	}

	public WorldObject GetTarget ()
	{
		return target;
	}

	[ClientCallback]
	protected virtual void OnMouseEnter ()
	{
		Texture2D texture = ResourceManager.GetCursor (0);
		if (!owner.isLocalPlayer) {
			Player localPlayer = Player.GetLocalPlayer ();
			if (localPlayer && (
			        (localPlayer.SelectedObject && localPlayer.SelectedObject.IsOwnedBy (localPlayer.netId) && localPlayer.SelectedObject.CanAttack ())
			        || localPlayer.SelectionUnits.Count > 0)) {
				texture = ResourceManager.GetCursor (1);
			}
		} 
	
		Cursor.SetCursor (texture, new Vector2 (texture.width / 2, texture.height / 2), CursorMode.Auto);
	}

	[ClientCallback]
	protected virtual void OnMouseExit ()
	{
		Cursor.SetCursor (null, Vector2.zero, CursorMode.Auto);
	}

	[ServerCallback]
	protected virtual void OnDestroy ()
	{
		foreach (Player p in GameManager.GetPlayers()) {
			if (p.SelectedObject == this)
				p.Select (null);
		}
		if (owner) {
			owner.playerList.Remove (this);
			if (hitPoints <= 0) {
				owner.statistics.AddDestroyed (this);
			}
		}
	}

	public void OnTriggerEnter (Collider other)
	{
		if (!target)
			return;
		Transform wo = other.transform.parent;

		if (state == WOState.Attacking && target.transform == wo) {
			targetInContact = true;
		}
	}

	public void OnTriggerExit (Collider other)
	{
		if (!target)
			return;
		Transform wo = other.transform.parent;

		if (state == WOState.Attacking && target.transform == wo) {
			targetInContact = false;
		}
	}
}
