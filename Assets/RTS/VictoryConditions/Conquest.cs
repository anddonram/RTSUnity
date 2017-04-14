using UnityEngine;
using System.Collections;

public class Conquest : VictoryCondition
{

	public override string GetDescription ()
	{
		return "el resto se murió";
	}

	/**tienen que quedar 2: mother y el otro jugador
	 */
	public override Player GetWinner ()
	{
		Player[] players = GetPlayers ();
		Player res = null;
		int left = players.Length;

		if (left > 2) {
			foreach (Player p in players)
				if (!PlayerMeetsCondition (p)) {
					left--;
				} else if (!p.mother) {
					res = p;
				}
		
			if (left > 2)
				res = null;
		}
		return res;
	}

	public override bool PlayerMeetsCondition (Player p)
	{
		return p && !p.IsDead ();
	}
}
