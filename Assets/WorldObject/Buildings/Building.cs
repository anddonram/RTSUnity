using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;
using UnityEngine.Networking;

public class Building : WorldObject
{
	public Transform spawnPoint;
	public Vector3 size;
	protected Queue< string > buildQueue;

	[SyncVar]
	public float
		maxBuildProgress;
	[SyncVar]
	protected float currentBuildProgress = 0.0f;

	protected Vector3 rallyPoint;
	protected WorldObject nextWo;
	public Node currentNode;

	protected override void Awake ()
	{
		base.Awake ();
		buildQueue = new Queue<string> ();
		renderers = GetComponentsInChildren<Renderer> (true);
	}

	[ServerCallback]
	protected override void Start ()
	{
		base.Start ();
		RaycastHit hit;
		Vector3 pos=spawnPoint.position;
		pos.y = -1;
		if (Physics.Raycast (pos, Vector3.up, out hit) && hit.collider.CompareTag ("Ground"))
			spawnPoint.position = hit.point;
		rallyPoint = spawnPoint.position + transform.forward * size.x;
	}

	[ServerCallback]
	protected override void Update ()
	{
		base.Update ();
		if (currentNode)
			DevelopNode ();
		else
			ProcessBuildQueue ();
	}

	protected override void OnGround (Vector3 position)
	{
		rallyPoint = position;
		nextWo = null;
		base.OnGround (position);
	}

	public override void PerformAction (string actionToPerform)
	{
		base.PerformAction (actionToPerform);
		if (owner.GetUnit (actionToPerform))
			owner.CmdCreateUnit (actionToPerform, gameObject);
		else {
			Node n = owner.GetNode (actionToPerform);
			if (n && owner.techTree.IsDevelopable (n))
				owner.CmdStartNode (n.gameObject, gameObject);
		}
	}

	public void StartNode (Node node)
	{
		currentNode = node;
		node.SetDeveloping (true);
		owner.AddResource (ResourceType.Money, -node.moneyCost);
		owner.AddResource (ResourceType.Water, -node.waterCost);

	}

	public void StopNode ()
	{
		if (currentNode) {
			currentNode.SetDeveloping (false);
			currentBuildProgress = 0;
			owner.AddResource (ResourceType.Money, currentNode.moneyCost);
			owner.AddResource (ResourceType.Water, currentNode.waterCost);
			currentNode = null;
		}
	}

	public void DevelopNode ()
	{
		currentBuildProgress += Time.deltaTime/currentNode.time;
		if (currentBuildProgress >= 1) {
			currentNode.Develop ();
			currentNode = null;
			currentBuildProgress = 0;

		}

	}

	protected override void OnWorldObject (WorldObject wo)
	{
		base.OnWorldObject (wo);
		nextWo = wo;
	}

	[Server]
	public virtual void Sell ()
	{
		owner.AddResource (ResourceType.Money, moneyCost / 4);
		owner.AddResource (ResourceType.Water, waterCost / 4);
		NetworkServer.Destroy (this.gameObject);
	}

	[Server]
	public virtual void Enter (Unit u)
	{
	}

	[Server]
	public virtual void Exit ()
	{
	}

	protected virtual void ProcessBuildQueue ()
	{
		if (buildQueue.Count > 0
		    && owner.GetResource (ResourceType.Population) < owner.GetResourceLimit (ResourceType.Population)) {
			currentBuildProgress += Time.deltaTime;
			if (currentBuildProgress >= maxBuildProgress) {
				Vector2 rnd = Random.insideUnitCircle * 3;
				Vector3 rndPos = new Vector3 (rnd.x, 0, rnd.y);

				owner.AddUnitFromString (buildQueue.Dequeue (), spawnPoint.position, transform.rotation, rallyPoint + rndPos, nextWo, running);
				currentBuildProgress = 0;

			}
		}
	}

	[Server]
	public virtual void CreateUnit (string unitName)
	{
		WorldObject wo = owner.GetUnit (unitName).GetComponent<WorldObject> ();
		if (owner.GetResource (ResourceType.Money) >= wo.moneyCost
		    && owner.GetResource (ResourceType.Water) >= wo.waterCost) {
			Consume (unitName);
			buildQueue.Enqueue (unitName);
		}
	}

	public bool UnderConstruction ()
	{
		return state == WOState.UnderConstruction;
	}

	public void StartConstruction ()
	{
		Consume (woName);
		state = WOState.UnderConstruction;
		hitPoints = 0;
	}

	public virtual void Construct (int amount)
	{
		hitPoints += amount;
		if (hitPoints >= maxHitPoints) {
			hitPoints = maxHitPoints;
			if (UnderConstruction ()) {
				state = WOState.Nothing;

				enabled = true;
				if(woName=="Hut"){
					owner.IncreaseResourceLimit (ResourceType.Money,50);
					owner.IncreaseResourceLimit (ResourceType.Water,25);
				}
				owner.RpcFinishBuilding (gameObject);
			}
		}

	}

	private float getBuildPercentage ()
	{
		return currentBuildProgress / maxBuildProgress;
	}

	private string[] getBuildQueueValues ()
	{
		string[] values = new string[buildQueue.Count];
		int pos = 0;
		foreach (string unit in buildQueue)
			values [pos++] = unit;
		return values;
	}

	public override string[] GetActions ()
	{
		List<string> res = new List<string> ();
		foreach (string s in actions)
			if (owner.GetUnit (s))
				res.Add (s);
			else {
				Node n = owner.GetNode (s);
				if (n && n.IsAvailable ())
					res.Add (s);
			}
		return res.ToArray ();
	}

	public override float GetActionPoints ()
	{
		return getBuildPercentage ();
	}

	public override string[] GetInfo ()
	{
		return getBuildQueueValues ();
	}

	public override Vector3 GetFlagPosition ()
	{
		if (nextWo)
			return nextWo.transform.position;
		return rallyPoint;
	}

	public override void OnStartClient ()
	{
		base.OnStartClient ();
		transform.SetParent (owner.buildings);
	}
	[ServerCallback]
	protected override void OnDestroy ()
	{
		base.OnDestroy ();
			Exit ();
			StopNode ();
	}

	private Renderer[] renderers;
	private List< Material > oldMaterials = new List< Material > ();
	public void RestoreMaterials ()
	{
		for (int i = 0; i < renderers.Length; i++) {
			renderers [i].material = oldMaterials [i];
		}
		oldMaterials.Clear();
	}

	public void SetTransparentMaterial (Material material)
	{
		foreach (Renderer renderer in renderers) {
			oldMaterials.Add (renderer.material);
			renderer.material = material;
		}
	}


}