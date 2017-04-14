using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
public abstract class VictoryCondition : NetworkBehaviour {

	public bool active;
	public Player[] GetPlayers(){
		return	GameManager.GetPlayers();
	}

	public virtual Player GetWinner(){
		foreach (Player p in GetPlayers())
			if (PlayerMeetsCondition (p))
				return p;
		return null;
	}
	public abstract string GetDescription ();
	public abstract bool PlayerMeetsCondition (Player p);
}
