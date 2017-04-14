using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class GameManager : NetworkBehaviour
{
	private VictoryCondition[] conditions;


	private static bool created = false;
	private float recalculate;
	private static Player[] players;


	void Awake ()
	{
		if (!created) {
			created = true;
			DontDestroyOnLoad (gameObject);
			conditions = GetComponents<VictoryCondition> ();
		} else
			Destroy (gameObject);
	
	}

	public void SetPlayers ()
	{
		players = FindObjectsOfType <Player> ();
		if (players == null)
			players = new Player[0];
	}

	public static Player[] GetPlayers ()
	{
		return players;
	}

	public static int GetPlayerNum ()
	{
		return players == null ? 0 : players.Length;
	}

	[ServerCallback]
	void Start ()
	{
		recalculate = 0;
	}

	[ServerCallback]
	void Update ()
	{
		recalculate += Time.deltaTime;
		if (recalculate > 2) {
			foreach (VictoryCondition v in conditions) {
				if (v.active) {
					Player p = v.GetWinner ();
					if (p) {
						CustomLobby.single.synchroManager.lastWinner = p.username;
						CustomLobby.single.synchroManager.lastWin = v.GetDescription ();
						//lobby.StopHost ();
						CustomLobby.single.ServerReturnToLobby ();
						break;
					}
				}	
			}
			recalculate = 0;
		}
	}



}
