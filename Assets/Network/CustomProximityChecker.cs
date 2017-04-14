using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class CustomProximityChecker : NetworkProximityChecker
{
	private WorldObject wo;

	void Awake ()
	{
		wo = GetComponent<WorldObject> ();
	}

	public override void OnSetLocalVisibility (bool vis)
	{
		base.OnSetLocalVisibility (vis);
		if (!(wo is Building)) {
			Collider[] cols = GetComponentsInChildren<Collider> ();
			foreach (Collider col in cols)
				col.enabled = vis;
		}
		if (vis == false && GameManager.GetPlayerNum () > 0) {

			foreach (Player p in GameManager.GetPlayers())
				if (p.SelectedObject == wo)
					p.Select (null);	
		}
	}

	public override bool OnRebuildObservers (HashSet<NetworkConnection> observers, bool initialize)
	{

		if (GameManager.GetPlayerNum () > 0 && wo.getOwner ()) {
			NetworkConnection conn;

			foreach (Player p in GameManager.GetPlayers ()) {
				if (!p)
					continue;
				conn = p.connectionToClient;
				if (p.human && conn != null) {
					if (wo.IsOwnedBy (p.netId) || p.spectator) {

						observers.Add (conn);
					} else {
						if (p is MockPlayer)
							continue;
					
						foreach (Unit u in  p.playerList.units) {
							if ((u.transform.position - transform.position).sqrMagnitude < u.prox.visRange * u.prox.visRange) {
								observers.Add (conn);
								break;
							}
						}
					
						if (observers.Contains (conn))
							continue;
						
						foreach (Building u in p.playerList.buildings) {
							if (!u.UnderConstruction () && (u.transform.position - transform.position).sqrMagnitude < u.prox.visRange * u.prox.visRange) {
								observers.Add (conn);
								break;
							}
						}

					}
				}
			}
		} 

		return true;
	}
}
