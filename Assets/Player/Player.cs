using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using RTS;
using UnityEngine.Networking;
using UnityEngine.EventSystems;

/**errores:
 * poner lineofsight sin que se solapen
 * poner cursores e info cuando pones el mouse encima
 * arreglar rock de una vez por todas
 * arreglar el combate cuerpo a cuerpo
 */
public class Player : NetworkBehaviour
{
	static Color blue= new Color (60/255f,175/255f,255/255f,100/255f);
	static Color red=new Color (225/255f,65/255f,65/255f,160/255f);
	/**
	 * datos del jugador: nombre,clan,color y si es o no humano
	 */
	[SyncVar]
	public string username;
	[SyncVar]
	public bool human, spectator;
	[SyncVar]
	public Clan clan;
	[SyncVar]
	public Color teamColor;
	/** exclusivo para ore, rocks, etc.
	 */
	public bool mother;
	/**
	 * recursos y limites de recursos iniciales
	 */
	[SyncVar]
	public int
		startMoney, startMoneyLimit,
		startWater, startWaterLimit, populationLimit;

	/**
	 * recursos y limites de recursos actuales
	 */
	[SyncVar]
	private int money, moneyLimit,
		water, waterLimit, population;

	/** interfaz del usuario
	 */
	public UI ui{ get; set; }

	/*
	 *objecto seleccionado
	 */
	public WorldObject SelectedObject{ get; set; }

	public List<Unit> SelectionUnits;
	/** bandera indicadora de destino de selectedobject
	 */
	public GameObject flag;
	/**
	 * esfera que indica el espacio de construcción
	 */
	public Transform buildSpace;
	public Renderer buildSpaceRend;
	/** para obtener las unidades y edificios del jugador
	 */
	public Transform units, buildings;
	/**
	 * arbol de tecnologias
	 */
	public TechTree techTree;
	/** lista de unidades y edificios
	 */
	public PlayerObjectList playerList;
	public PlayerStatistics statistics;
	private bool surrendered = false;
	private bool started = false;


	protected virtual void Start ()
	{
		buildSpaceRend = buildSpace.GetComponent<Renderer> ();
		SelectionUnits = new List<Unit> ();
		AddStartResourceLimits ();
		AddStartResources ();
		if (isServer&&!human && !mother) {
			techTree = (TechTree)Instantiate (ResourceManager.GetTechTree (clan), transform.position, transform.rotation);
			techTree.player = this;
			InitSpawning ();
		}
	}

	/** actualizar la ui y construir un edificio si se puede
	 */
	[ClientCallback]
	protected virtual void  Update ()
	{
		if (!isLocalPlayer)
			return;
	
		if (SelectedObject) {
			ui.UpdateHealth (SelectedObject);
			ui.UpdateActionBar (SelectedObject);
			if (SelectedObject is Unit)
				ui.UpdateStaminaBar (SelectedObject);
		}
			
	
		if (IsFindingBuildingLocation ()) {
			bool canPlace = CanPlaceBuilding ();
			if (canPlace != canPlaceLast) {
				if (canPlace) {
					buildSpaceRend.material.color = blue;
				} else {
					buildSpaceRend.material.color =red;
				}
				canPlaceLast = canPlace;	
			}

		}
	
	}

	/** seleccionar
	 * si se recibió el objeto se selecciona y se indica al server que seleccione
	 */
	[Server]
	public void Select (WorldObject wo)
	{
		if (wo) {
			SelectedObject = wo;
			RpcSelect (wo.gameObject);
		} else
			RpcSelect (gameObject);
	}

	/** commando al servidor para seleccionar
	 */
	[Command]
	public void CmdSelect (GameObject g)
	{
		Select (g.GetComponent<WorldObject> ());
	}
	/*Llamada al cliente para selccionar. Solo el local selecciona y actualiza su HUD y su bandera
	 */
	[ClientRpc]
	public void RpcSelect (GameObject g)
	{
		if (!isLocalPlayer)
			return;

		if (SelectedObject) {
			SelectedObject.SetSelection (false);
		}	
		ui.ClearHUD ();
		SelectedObject = g.GetComponent<WorldObject> ();

		if (SelectedObject) {
			ui.UpdateHUD (SelectedObject);
			SelectedObject.SetSelection (true);
			if (SelectedObject.IsOwnedBy (netId))
				SetFlag (SelectedObject.GetFlagPosition ());
			else
				SetFlag (ResourceManager.InvalidPosition);
		} else {
			SetFlag (ResourceManager.InvalidPosition);
		}
	}


	private void AddStartResourceLimits ()
	{
		moneyLimit = startMoneyLimit;
		waterLimit = startWaterLimit;
		if (isLocalPlayer)
			foreach (ResourceType t in ResourceManager.ResourceTypes) {
				ui.UpdateResourceLimitValues (t);
			}
		
	}

	private void AddStartResources ()
	{
		money = startMoney;
		water = startWater; 
		population = units.childCount;
		if (isLocalPlayer)
			foreach (ResourceType t in ResourceManager.ResourceTypes) {
				ui.UpdateResourceValues (t);
			}
	}

	public void AddResource (ResourceType type, int amount)
	{
		int increment = 0;
		switch (type) {
		case ResourceType.Money:
			increment = Mathf.Clamp (amount, -money, moneyLimit-money);
			money += increment; 
			break;
		case ResourceType.Water:
			increment=Mathf.Clamp (amount, -water, waterLimit-water);
			water += increment;
			break;
		case ResourceType.Population:
			population = population + amount;
			break;
		}
		if(increment!=0)
			statistics.AddResource (type,increment);
		if (human)
			RpcUpdateResourceValues (type);
		
	}

	public void IncreaseResourceLimit (ResourceType type, int amount)
	{
		switch (type) {
		case ResourceType.Money:
			moneyLimit += amount;
			break;
		case ResourceType.Water:
			waterLimit += amount;
			break;
		case ResourceType.Population:
			populationLimit += amount;
			break;
		}
		if (human)
			RpcUpdateResourceLimitValues (type);
	
		AddResource (type, 0);
	}

	public int GetResource (ResourceType type)
	{
		int res = 0;
		switch (type) {
		case ResourceType.Money:
			res = money;
			break;
		case ResourceType.Water:
			res = water;
			break;
		case ResourceType.Population:
			res = population;
			break;
		}
		return res;
	}

	public int GetResourceLimit (ResourceType type)
	{
		int res = 0;
		switch (type) {
		case ResourceType.Money:
			res = moneyLimit;
			break;
		case ResourceType.Water:
			res = waterLimit;
			break;
		case ResourceType.Population:
			res = populationLimit;
			break;
		}
		return res;
	}

	public void SetFlag (Vector3 flagPos)
	{
		if (flagPos == ResourceManager.InvalidPosition) {
			flag.SetActive (false);
		} else {
			flag.SetActive (true);
			flag.transform.position = Vector3.up + flagPos;
		}
	}

	[ClientRpc]
	public void RpcSetFlag (Vector3 flag)
	{
		if (!isLocalPlayer)
			return;
		SetFlag (flag);
	}

	public void AddUnitFromString (string unitName, Vector3 spawnPoint, Quaternion rotation, Vector3 rallyPoint, WorldObject next, bool run)
	{
		GameObject newUnit = (GameObject)Instantiate (GetUnit (unitName), spawnPoint, rotation);
		SetUnitProperties (newUnit, rallyPoint, next, run);
	}

	public void AddUnit (int unitValue, Vector3 spawnPoint, Quaternion rotation, Vector3 rallyPoint, WorldObject next, bool run)
	{
		GameObject newUnit = (GameObject)Instantiate (GetUnit (unitValue), spawnPoint, rotation);
		SetUnitProperties (newUnit, rallyPoint, next, run);
	}

	void SetUnitProperties (GameObject newUnit, Vector3 rallyPoint, WorldObject next, bool run)
	{
		SetWoProperties (newUnit);
		NetworkServer.Spawn (newUnit);
		AddResource (ResourceType.Population, 1);
		Unit unit = newUnit.GetComponent<Unit> ();
		if (next) {
			unit.MouseClick (next.gameObject, rallyPoint, netId, run);
		} else {
			unit.StartMove (rallyPoint);
			unit.Run (run);
		}
	}


	private Building tempBuilding;
	private Unit tempCreator;
	private bool
		canPlaceLast = false;


	public void CreateBuilding (string buildingName, Vector3 buildPoint, Unit creator)
	{
		if (tempBuilding)
			CancelBuilding ();
		GameObject newBuilding = GetBuilding (buildingName);
		tempBuilding = newBuilding.GetComponent< Building > ();
		tempCreator = creator;

		buildSpace.position = buildPoint;
		if (!isLocalPlayer)
			return;
		buildSpace.localScale = 2 * tempBuilding.size;
		buildSpace.gameObject.SetActive (true);
		canPlaceLast = CanPlaceBuilding ();
		if (canPlaceLast) {
			buildSpaceRend.material.color = blue;
		} else {
			buildSpaceRend.material.color = red;
		}
	}

	public bool IsFindingBuildingLocation ()
	{
		return tempBuilding;
	}

	public void FindBuildingLocation ()
	{
		Vector3 newLocation = WorkManager.FindHitPoint ();
		if (newLocation != ResourceManager.InvalidPosition) {
			buildSpace.position = new Vector3 (Mathf.Round (newLocation.x),
				Mathf.Round (newLocation.y),
				Mathf.Round (newLocation.z));
		}
	}

	public bool CanPlaceBuilding ()
	{
		if (GetResource (ResourceType.Money) < tempBuilding.moneyCost ||
		    GetResource (ResourceType.Water) < tempBuilding.waterCost ||
		    buildSpace.position == ResourceManager.InvalidPosition)
			return false;
	
		Collider[] col = Physics.OverlapBox (buildSpace.position, tempBuilding.size, Quaternion.identity, 1 + (1 << 4) + (1 << 10));
		Transform t;
		foreach (Collider c in col) {
			t = c.transform.parent;

			if (!t || !(t.GetComponent<Unit> ()))
				return false;
		}
		return true;
	}

	public void CancelBuilding ()
	{
		tempCreator = null;
		tempBuilding = null;
		buildSpace.gameObject.SetActive (false);
	}

	public void StartConstruction ()
	{
		CmdConstruction (tempBuilding.woName, buildSpace.position, tempCreator.gameObject);
		CancelBuilding ();
	}

	public void Sell ()
	{
		if (SelectedObject is Building) {
			Building b = (Building)SelectedObject;
			b.Sell ();
		}
	}

	public void Cancel ()
	{
		if (SelectedObject is Building) {
			Building b =
				(SelectedObject as Building);
			if (b.currentNode)
				b.StopNode ();
			else
				b.Exit ();
		}
	}

	[Command]
	public void CmdCancel ()
	{
		Cancel ();
	}

	[Command]
	public void CmdSell ()
	{
		Sell ();
	}

	public void CancelCommand ()
	{
		CmdCancel ();
	}

	public void SellCommand ()
	{
		CmdSell ();
	}

	public void ActionButton (int action)
	{
		if (SelectedObject && action < SelectedObject.GetActions ().Length) {
			SelectedObject.PerformAction (SelectedObject.GetActions () [action]);
			ui.UpdateActionPanel (SelectedObject);
		} else if (SelectionUnits.Count > 1 && action < SelectionUnits.Count) {
			if (Input.GetKey (KeyCode.RightShift)) {
				SelectionUnits [action].SetSelection (false);
				SelectionUnits.RemoveAt (action);
				if (SelectionUnits.Count == 1) {
					Unit sel = SelectionUnits [0];
					SelectionUnits.Clear ();
					CmdSelect (sel.gameObject);
				}
				ui.UpdateSelectionBox ();
			} else
				SetCamera (SelectionUnits [action]);
		}
	}

	[Command]
	public void CmdCreateUnit (string unitName, GameObject building)
	{
		building.GetComponent<Building> ().CreateUnit (unitName);
	}

	[Command]
	public void CmdStartNode (GameObject node, GameObject building)
	{
		building.GetComponent<Building> ().StartNode (node.GetComponent<Node> ());
	}

	[Command]
	public void CmdKill ()
	{
		if (SelectedObject && SelectedObject.IsOwnedBy (netId)) {
			NetworkServer.Destroy (SelectedObject.gameObject);
		}
	}

	public void SetCamera (Unit u)
	{
		UserInput input = GetComponent<UserInput> ();
		input.ClearSelection ();
		if (!u) {
			CmdSelect (gameObject);
			return;
		}
		Ray r = input.cam.ViewportPointToRay (new Vector3 (.5f, .5f, 0));
		RaycastHit hit;

		if (Physics.Raycast (r, out hit)) {
			Vector3 click = hit.point;
			Vector3 dir = u.transform.position - click;
			input.cam.transform.position += dir;
		}
		CmdSelect (u.gameObject);

	}

	[Command]
	void CmdConstruction (string buildingName, Vector3 pos, GameObject creator)
	{
		Construction (buildingName, pos, creator);
	}

	public void Construction (string buildingName, Vector3 pos, GameObject creator)
	{
		GameObject newBuilding = (GameObject)Instantiate (GetBuilding (buildingName), pos, Quaternion.identity);
		Building b = newBuilding.GetComponent<Building> ();
		SetWoProperties (newBuilding);
		NetworkServer.Spawn (newBuilding);
		b.enabled = false;
		RpcSetTransparentMaterial (newBuilding);
		b.StartConstruction ();
		creator.GetComponent<Unit> ().MouseClick (newBuilding, Vector3.zero, netId, false);

	}

	[ClientRpc]
	public void RpcRain ()
	{
		FindObjectOfType<RainController> ().Rain ();
	}

	[ClientRpc]
	void RpcSetTransparentMaterial (GameObject g)
	{
		Building b = g.GetComponent<Building> ();
		b.lineOfSight.enabled = false;
		b.SetTransparentMaterial (ResourceManager.GetAllowedMaterial());
	}

	[ClientRpc]
	public void RpcFinishBuilding (GameObject g)
	{
		Building b =
			g.GetComponent<Building> ();
		b.RestoreMaterials ();
		b.enabled = true;
		if (isLocalPlayer)
			b.lineOfSight.enabled = true;
		if (b is Well) {
			(b as Well).water.gameObject.SetActive (true);
		}
	}

	public GameObject GetUnit (string unitName)
	{
		return ResourceManager.GetUnit (unitName, clan);
	}

	public GameObject GetUnit (int unitValue)
	{
		return ResourceManager.GetUnit (unitValue, clan);
	}

	public GameObject GetBuilding (string buildingName)
	{
		return ResourceManager.GetBuilding (buildingName, clan);
	}

	public GameObject GetBuilding (int buildingValue)
	{
		return ResourceManager.GetBuilding (buildingValue, clan);
	}

	public Node GetNode (string nodeName)
	{
		return techTree.GetNode (nodeName);
	}

	public int GetUnitListCount(){
		return ResourceManager.GetUnitListCount (clan);
	}
	public int GetBuildingListCount(){
		return ResourceManager.GetBuildingListCount (clan);
	}
	public override void OnStartLocalPlayer ()
	{
		localPlayer = this;
		ui = Instantiate (ResourceManager.GetCanvas ()).GetComponent<UI> ();
		ui.player = this;
	
		for (int i = 0; i < ui.actionButtonList.Length; i++) {
			int x = i;
			ui.actionButtonList [i].onClick.AddListener (() => ActionButton (x));
		}
		ui.sellButton.GetComponent<Button> ().onClick.AddListener (SellCommand);
		ui.cancelButton.GetComponent<Button> ().onClick.AddListener (CancelCommand);
	
		CmdInitSpawning ();
		PauseMenu pM = GetComponent<PauseMenu> ();
		pM.menuPanel = ui.menuPanel;
		Button[] b = pM.menuPanel.GetComponentsInChildren<Button> ();
		b [0].onClick.AddListener (pM.Resume);
		b [1].onClick.AddListener (pM.ExitGame);
	}

	[Command]
	void CmdInitSpawning ()
	{ 
		InitSpawning ();
	}

	void InitSpawning ()
	{
		GameObject newBuilding = (GameObject)Instantiate (GetBuilding ("Hut"), transform.position, transform.rotation);

		SetWoProperties (newBuilding);
		NetworkServer.Spawn (newBuilding);
		started = true;
	}

	public void SetWoProperties (GameObject g)
	{
		WorldObject wo = g.GetComponent<WorldObject> ();
		wo.SetPlayer (this);
		playerList.Add (wo);
		statistics.AddCreated (wo);
		if (wo is Building) {
			wo.transform.SetParent (buildings);

		} else {
			wo.transform.SetParent (units);
		}
	}

	[Command]
	public void CmdClick (GameObject hitObject, Vector3 hitPoint, bool run)
	{
		SelectedObject.MouseClick (hitObject, hitPoint, netId, run);
	}

	[Command]
	void CmdSpawn (GameObject g)
	{
		NetworkServer.Spawn (g);
	}

	[Command]
	public void CmdLog (string s)
	{
		Debug.Log (s);
	}

	[ClientRpc]
	public void RpcLog (string s)
	{
		Debug.Log (s);
	}

	[ClientRpc]
	public void RpcSetActive (GameObject g, bool active)
	{
		g.SetActive (active);
	}

	[ClientRpc]
	public void RpcPush (GameObject g)
	{
		if (!isLocalPlayer)
			return;
		CmdPush (g.GetComponent<Rock> ().GetDirection (), g);
	}

	[Command]
	public void CmdPush (Vector3 dir, GameObject g)
	{
		g.GetComponent<Rock> ().Push (dir);
	}

	[ClientRpc]
	public void RpcActivate (GameObject g, bool active)
	{
		g.GetComponent<UnityEngine.AI.NavMeshAgent> ().enabled = active;
		foreach (Collider col in g.GetComponentsInChildren<Collider>()) {
			col.enabled = active;
		}
	}

	[ClientRpc]
	public void RpcUpdateResourceLimitValues (ResourceType type)
	{
		if (isLocalPlayer)
			ui.UpdateResourceLimitValues (type);
		
	}

	[ClientRpc]
	public void RpcUpdateResourceValues (ResourceType type)
	{
		if (isLocalPlayer)
			ui.UpdateResourceValues (type);
		
	}

	[Command]
	public void CmdSpecialMove ()
	{
		Unit u = SelectedObject as Unit;
		if (u.specialActivated)
			u.SpecialMove ();
	}

	[Command]
	public void CmdStopActions ()
	{
		Unit u = SelectedObject as Unit;	
		u.StopActions ();
	}

	public  virtual bool IsDead ()
	{
		return started && !mother && (surrendered || (population == 0 && buildings.childCount == 0));
	}

	public bool HasResources (WorldObject wo)
	{
		return wo.getOwner ().GetResource (ResourceType.Money) >= wo.moneyCost
		&& wo.getOwner ().GetResource (ResourceType.Water) >= wo.waterCost;
	}

	public Vector3 FindRandomPlace (int tries, Vector3 place)
	{
		int count = buildings.childCount + 1;
		Vector2 rnd = Random.insideUnitCircle * tries * count;
		Vector3 res = place + new Vector3 (rnd.x, 0, rnd.y);
		res.y = -1;
		RaycastHit hit;
		if (Physics.Raycast (res, Vector3.up, out hit, 100, 1 << 8)) {
			res = hit.point;
			buildSpace.position = res;
		} else {
			res = ResourceManager.InvalidPosition;
		}

		return res;
	}

	public void Surrender ()
	{
		if (!surrendered)
			surrendered = true;
		if (human) {
			Time.timeScale = 1.0f;
			if (CustomLobby.single.isHost) {
				CustomLobby.single.ServerReturnToLobby ();
			} else {
				CustomLobby.single.StopClient ();
			}
		}
	}

	protected static Player localPlayer;

	public static Player GetLocalPlayer ()
	{
		return localPlayer;
	}
	void OnGUI(){
		if (isLocalPlayer&&GUI.Button (new Rect (0, 0, 60, 30), "add")) {
			AddResource (ResourceType.Money,400);
			AddResource (ResourceType.Water,400);
		}
	}	
}
