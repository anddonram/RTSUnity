using RTS;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class UserInput : NetworkBehaviour
{
	private Player player;
	public Camera cam;
	private Transform camTransform;

	private Camera minimap;

	[SyncVar]
	private Vector3 min, max;
	private float height=ResourceManager.MinCameraHeight;
	void Awake ()
	{
		player = GetComponent<Player> ();
	}


	[ClientCallback]
	void Update ()
	{
		if (!isLocalPlayer) {
			return;
		}
		click += Time.deltaTime;
		if (click >= betweenClick) {
			firstClick = false;
		}
		if (Input.GetKeyDown (KeyCode.Escape))
			OpenPauseMenu ();
		else if (Input.GetKeyDown (KeyCode.B))
			ActivateSpecial ();
		else if (Input.GetKeyDown (KeyCode.Space))
			CenterCameraOnUnit ();
		else if (Input.GetKeyDown (KeyCode.Delete))
			player.CmdKill ();
		else if (Input.GetKeyDown (KeyCode.S))
			StopActions ();
		if (MouseOverMinimap () && !onGame) {
			if (Input.GetMouseButton (0))
				MoveCameraWithMinimap ();
			else if (Input.GetMouseButton (1)) {
				RightClick (true);
			}
		} else {
			MoveCamera ();
			RotateCamera ();
			if (player.IsFindingBuildingLocation () && !EventSystem.current.IsPointerOverGameObject ()) {
				Construction ();
			} else {
				MouseActivity ();
			}
		}
		
	}

	private void CenterCameraOnUnit ()
	{

		if (player.SelectedObject) {
			Ray r = cam.ViewportPointToRay (new Vector3 (.5f, .5f, 0));
			RaycastHit hit;
		
			if (Physics.Raycast (r, out hit)) {
				Vector3 click = hit.point;
				Vector3 dir = player.SelectedObject.transform.position - click;
				camTransform.position += dir;
			
			}
		} 
	}
	Plane p=new Plane (Vector3.up, Vector3.zero);
	private bool MouseOverMinimap ()
	{
		Vector3 point = minimap.ScreenToViewportPoint (Input.mousePosition);
		return point.x >= 0 && point.x <= 1 && point.y >= 0 && point.y <= 1;
	}

	private void MoveCameraWithMinimap ()
	{
		Vector3 point = minimap.ScreenToWorldPoint (Input.mousePosition);
		point.y = height+Terrain.activeTerrain.SampleHeight (point);
		Vector3 clampMin = min;
		Vector3 clampMax = max;
		Ray r = cam.ViewportPointToRay (new Vector3 (.5f, .5f, 0));

		float dist = 0;
		if (p.Raycast (r, out dist)) {
			Vector3 clamp = camTransform.position - r.GetPoint (dist);
			clampMin += clamp;
			clampMax += clamp;
		}
		point.x = Mathf.Clamp (point.x, clampMin.x, clampMax.x);
		point.z = Mathf.Clamp (point.z, clampMin.z, clampMax.z);

		camTransform.position = point;

	}

	private void MoveCamera ()
	{
		float xpos = Input.mousePosition.x;
		float ypos = Input.mousePosition.y;
		Vector3 movement = Vector3.zero;
		if (xpos >= 0 && xpos < ResourceManager.ScrollWidth) {
			movement.x -= ResourceManager.ScrollSpeed;
		} else if (xpos <= Screen.width && xpos > Screen.width - ResourceManager.ScrollWidth) {
			movement.x += ResourceManager.ScrollSpeed;
		}
		
		if (ypos >= 0 && ypos < ResourceManager.ScrollWidth) {
			movement.z -= ResourceManager.ScrollSpeed;
		} else if (ypos <= Screen.height && ypos > Screen.height - ResourceManager.ScrollWidth) {
			movement.z += ResourceManager.ScrollSpeed;
		}
		movement.x += Input.GetAxisRaw ("Horizontal") * ResourceManager.ScrollSpeed;

		movement.z += Input.GetAxisRaw ("Vertical") * ResourceManager.ScrollSpeed;

		movement = camTransform.TransformDirection (movement);
		movement.y -=ResourceManager.ScrollSpeed * Input.GetAxis ("Mouse ScrollWheel");
		if (movement != Vector3.zero) {
			Vector3 origin = camTransform.position;
			Vector3 destination = origin+movement;
			Vector3 clampMin = min;
			Vector3 clampMax = max;
			height = Mathf.Clamp (height+movement.y,ResourceManager.MinCameraHeight,ResourceManager.MaxCameraHeight);
			float camHeight=Terrain.activeTerrain.SampleHeight (origin);

			Ray r = cam.ViewportPointToRay (new Vector3 (.5f, .5f, 0));
			float dist = 0;
			if (p.Raycast (r, out dist)) {
				Vector3 clamp = camTransform.position - r.GetPoint (dist);
				clampMin += clamp;
				clampMax += clamp;
			}
			destination.x = Mathf.Clamp (destination.x, clampMin.x, clampMax.x);
			destination.y = height + camHeight;
			destination.z = Mathf.Clamp (destination.z, clampMin.z, clampMax.z);

			if (destination != origin) {
				camTransform.position = Vector3.MoveTowards (origin, destination, Time.deltaTime *
				ResourceManager.ScrollSpeed);
			}
		}
	}

	private void RotateCamera ()
	{
		Vector3 origin = camTransform.eulerAngles;
		Vector3 destination = origin;
		if ((Input.GetKey (KeyCode.LeftAlt) || Input.GetKey (KeyCode.RightAlt))
		    && Input.GetMouseButton (1)) {
			destination.x -= Input.GetAxis ("Mouse Y") * ResourceManager.RotateAmount;
			destination.y += Input.GetAxis ("Mouse X") * ResourceManager.RotateAmount;
			destination.x = Mathf.Clamp (destination.x, ResourceManager.MinCameraAngle, ResourceManager.MaxCameraAngle);
		}
		if (destination != origin)
			camTransform.eulerAngles = Vector3.MoveTowards (origin, destination, Time.deltaTime * ResourceManager.RotateSpeed);
	}


	private RectTransform box;
	private Vector3 origin;
	private bool selecting, onGame;

	private void MouseActivity ()
	{
		Vector3 current = Input.mousePosition;
		if (Input.GetMouseButtonDown (0))
		if (!EventSystem.current.IsPointerOverGameObject ()) {
			player.CmdSelect (gameObject);
			ClearSelection ();
			origin = current;
			onGame = true;
		}
		if (Input.GetMouseButtonUp (0)) {
			if (selecting) {
				selecting = false;
				SelectViewport ();
				origin = Vector3.zero;
				box.anchorMin = box.anchorMax;
			} else {
				LeftClick ();
			}
			onGame = false;
		} else if (Input.GetMouseButton (0)) {
			
			if (onGame)
			if (selecting || origin != current) {
				selecting = true;
			
				Vector2 startPoint = origin;
				Vector2 difference = current - origin;
				if (difference.x < 0) {
					startPoint.x = current.x;
					difference.x = -difference.x;
				}
				if (difference.y < 0) {
					startPoint.y = current.y;
					difference.y = -difference.y;
				}

				box.anchorMin = new Vector2 (startPoint.x / Screen.width, startPoint.y / Screen.height);
				box.anchorMax = new Vector2 ((startPoint.x + difference.x) / Screen.width, (startPoint.y + difference.y) / Screen.height);
		
			}
		}

		if (Input.GetMouseButtonDown (1))
			RightClick (false);
	}

	/*seleccionar*/
	void LeftClick ()
	{	
		GameObject hitObject;
		Vector3 hitPoint;
		FindHit (false, out hitObject, out hitPoint);
		if (hitObject) {
			if (hitObject.CompareTag ("Ground"))
				player.CmdSelect (hitObject);
			else
				player.CmdSelect (hitObject.transform.parent.gameObject);
		}
	}

	private float click;
	private float betweenClick = 0.2f;
	private bool firstClick;

	/*acciones*/
	void RightClick (bool mini)
	{
		if (!Input.GetKey (KeyCode.LeftAlt) && !Input.GetKey (KeyCode.RightAlt) && (player.SelectionUnits.Count > 0 || player.SelectedObject)) {
			bool run = firstClick;
			if (!firstClick) {
				click = 0;
				firstClick = true;
			} else {

				firstClick = false;
			}
			GameObject hitObject;
			Vector3 hitPoint;
			FindHit (mini, out hitObject, out hitPoint);

			if (hitObject && !hitObject.CompareTag ("Ground"))
				hitObject = hitObject.transform.parent.gameObject;

			if (player.SelectionUnits.Count == 0) {	
				player.CmdClick (hitObject, hitPoint, run);
			} else {
				HandleGroup (hitObject, hitPoint, run);
			}
		}
	}

	public void ClearSelection ()
	{
		foreach (Unit u in player.SelectionUnits)
			u.SetSelection (false);
		player.SelectionUnits.Clear ();
	}

	private void HandleGroup (GameObject hitObject, Vector3 hitPoint, bool run)
	{
		Vector3 offset;
		Vector2 rnd;
		foreach (Unit u in player.SelectionUnits) {
			rnd = Random.insideUnitCircle * player.SelectionUnits.Count;
			offset = new Vector3 (rnd.x, 0, rnd.y);
			CmdGroupClick (u.gameObject, hitObject, hitPoint + offset, run);

		}
	}

	[Command]
	void CmdGroupClick (GameObject unit, GameObject sendObject, Vector3 offset, bool run)
	{
		unit.GetComponent<WorldObject> ().MouseClick (sendObject, offset, netId, run);

	}

	private void Construction ()
	{
		player.FindBuildingLocation ();
		if (Input.GetMouseButtonDown (1))
			player.CancelBuilding ();
		else if (Input.GetMouseButtonUp (0) && player.CanPlaceBuilding ())
			player.StartConstruction ();
	}

	private void FindHit (bool mini, out GameObject hitObject, out Vector3 hitPoint)
	{
		hitObject = null;
		hitPoint = ResourceManager.InvalidPosition;
		if (EventSystem.current.IsPointerOverGameObject ())
			return;
		Ray r;
		if (mini)
			r = minimap.ScreenPointToRay (Input.mousePosition);
		else
			r = cam.ScreenPointToRay (Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast (r, out hit, 100, (1 << 8) + (1 << 4) + 1)) {
			hitObject = hit.collider.gameObject;
			hitPoint = hit.point;
		}
	}

	private void OpenPauseMenu ()
	{
		if (Time.timeScale > 0) {
			GetComponent<PauseMenu> ().enabled = true;
			CmdStopGame ();
		}
	}

	[Command]
	void CmdRightClick ()
	{		
		RightClick (false);
	}

	[Command]
	void CmdStopGame ()
	{
		RpcStopAll ();
	}

	[ClientRpc]
	void RpcStopAll ()
	{
		enabled = false;
		Time.timeScale = 0.0f;
	}
	private Bounds bounds= new Bounds ();
	Bounds GetViewportBounds (Vector3 screenPosition1, Vector3 screenPosition2)
	{
		Vector3 v1 = cam.ScreenToViewportPoint (screenPosition1);
		Vector3 v2 = cam.ScreenToViewportPoint (screenPosition2);
		Vector3 min = Vector3.Min (v1, v2);
		Vector3 max = Vector3.Max (v1, v2);
		min.z = cam.nearClipPlane;
		max.z = cam.farClipPlane;

		bounds.SetMinMax (min, max);
		return bounds;
	}

	bool IsWithinSelectionBounds (GameObject gameObject)
	{		
		return GetViewportBounds (origin, Input.mousePosition).Contains (
			cam.WorldToViewportPoint (gameObject.transform.position));
	}

	void SelectViewport ()
	{
		foreach (Unit u in player.playerList.units) {
			if (!player.SelectionUnits.Contains (u) && u.state != WOState.Entering && IsWithinSelectionBounds (u.gameObject)) {
				player.SelectionUnits.Add (u);
				u.SetSelection (true);
			}
		}
		if (player.SelectionUnits.Count == 1) {
			Unit sel = player.SelectionUnits [0];
			ClearSelection ();	
			player.CmdSelect (sel.gameObject);
		} else if (player.SelectionUnits.Count > 1)
			player.ui.UpdateSelectionBox ();
	}

	void ActivateSpecial ()
	{
		if (player.SelectedObject is Unit && player.SelectedObject.IsOwnedBy (netId)) {
			player.CmdSpecialMove ();
		} else {
			foreach (Unit u in player.SelectionUnits) {
				if (u.specialActivated)
					CmdSpecialMove (u.gameObject);
			}
		}
	}

	[Command]
	void CmdSpecialMove (GameObject u)
	{
		u.GetComponent<Unit> ().SpecialMove ();
	}
	void StopActions ()
	{
		if (player.SelectedObject is Unit && player.SelectedObject.IsOwnedBy (netId)) {
			player.CmdStopActions ();
		} else {
			foreach (Unit u in player.SelectionUnits) {
					CmdStopActions (u.gameObject);
			}
		}
	}
	[Command]
	void CmdStopActions (GameObject u)
	{
		u.GetComponent<Unit> ().StopActions ();
	}

	public override void OnStartLocalPlayer ()
	{
		cam = Camera.main;
		camTransform = cam.transform;
		camTransform.position += transform.position;
		minimap = GameObject.FindGameObjectWithTag ("Minimap").GetComponent<Camera> ();
		minimap.aspect = 1;
		box = GameObject.FindGameObjectWithTag ("Box").GetComponent<RectTransform> ();

	}
	public void SetMinMax(Vector3 min,Vector3 max){
		this.min = min;
		this.max = max;
	}

}
