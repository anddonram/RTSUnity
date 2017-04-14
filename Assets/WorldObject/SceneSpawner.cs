using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
/**
 * este componente debe ir antes del world object
 */
public class SceneSpawner : NetworkBehaviour {

	public Player player;
	public WorldObject wo;
	public override void OnStartServer ()
	{
		wo.SetPlayer(player);
		player.playerList.Add (wo);
	
	}

	public override void OnStartClient ()
	{
		if (!isServer) 
			wo.SetPlayer (player);

	}
}
