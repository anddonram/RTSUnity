using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using RTS;
using UnityEngine.UI;

public class CustomLobby :  NetworkLobbyManager
{
	public GameManager gameManager;
	public SynchroManager synchroManager;
	public GameObject mockPlayer;
	public UIHandler uiHandler;
	public bool isHost=false;
	public static CustomLobby single;
	void Start()
	{
		single = this;
	}
	public override bool OnLobbyServerSceneLoadedForPlayer (GameObject lobbyPlayer, GameObject gamePlayer)
	{
		Control cc = lobbyPlayer.GetComponent<Control> ();
		Player player = gamePlayer.GetComponent<Player> ();

		player.username = cc.username;
		player.spectator = cc.isSpectator;

		player.teamColor = cc.color;
		player.clan = cc.clan;
		player.techTree = (TechTree)Instantiate (ResourceManager.GetTechTree (player.clan), transform.position, transform.rotation);
		player.techTree.player = player;


//		player.startMoney;
		player.startMoneyLimit = synchroManager.moneyLimit;
//		player.startWater;
		player.startWaterLimit = synchroManager.waterLimit;
		player.populationLimit = synchroManager.populationLimit;
		return true;
	}
	public override void OnLobbyClientSceneChanged (NetworkConnection conn)
	{
		bool toLobby = networkSceneName == lobbyScene;
		if (toLobby) {
			Cursor.SetCursor (null,Vector2.zero,CursorMode.Auto);
			gameManager.SetPlayers ();
			uiHandler.SetFirstPanel (false);
			uiHandler.readyButton.text="Ready";
		} 
		uiHandler.SetUI (toLobby);
		gameManager.enabled = !toLobby;
	}

	public override void OnLobbyServerSceneChanged (string scene)
	{
		gameManager.SetPlayers ();
		if (networkSceneName == lobbyScene) {
			gameManager.enabled = false;
			return;
		}
		Terrain terrain = Terrain.activeTerrain;
		AIHandler[] handlers = FindObjectsOfType<AIHandler> ();
		foreach (AIHandler ai in handlers) {
			ai.terrain = terrain;
			ai.player.startMoneyLimit = synchroManager.moneyLimit;
			ai.player.startWaterLimit = synchroManager.waterLimit;
			ai.player.populationLimit = synchroManager.populationLimit;
		}
	
		UserInput[] inputs = FindObjectsOfType<UserInput> ();
		Vector3 min = terrain.transform.position;
		Vector3 max = min + terrain.terrainData.size;
		foreach (UserInput ui in inputs) {
			ui.SetMinMax (min, max);
		}
		gameManager.enabled = true;
		synchroManager.lastWin = string.Empty;
		synchroManager.lastWinner = string.Empty;
	}

	public override GameObject OnLobbyServerCreateGamePlayer(NetworkConnection conn,short playerControllerId){
			
		GameObject player;
		if (conn.playerControllers [0].gameObject.GetComponent<Control> ().isMock) {
			player = Instantiate (mockPlayer);
		} else {
			player = base.OnLobbyServerCreateGamePlayer (conn, playerControllerId);
		}
			return player;
	}
	public override void OnLobbyServerPlayersReady()
	{
		bool allready = true;
		for(int i = 0; i < lobbySlots.Length; ++i)
		{
			if(lobbySlots[i] != null)
				allready &= lobbySlots[i].readyToBegin;
		}

		if (allready)
			ServerChangeScene (playScene);
			//StartCoroutine(ServerCountdownCoroutine());
	}

//	public override void OnLobbyClientConnect (NetworkConnection conn)
//	{
//		base.OnLobbyClientConnect (conn);
//		FindObjectOfType<UIHandler> ().SetFirstPanel (false);
//	}
	public override void OnLobbyClientDisconnect (NetworkConnection conn)
	{
		base.OnLobbyClientDisconnect (conn);
		uiHandler.SetFirstPanel (true);
		if (!isHost) {
			StopClient ();
		}
	}
	public override void OnLobbyClientAddPlayerFailed ()
	{
		base.OnLobbyClientAddPlayerFailed ();
		uiHandler.SetFirstPanel (true);
	}
	public override void OnLobbyStartClient (NetworkClient lobbyClient)
	{
		base.OnLobbyStartClient (lobbyClient);
		uiHandler.SetStopPanel ();
	}
	public override void OnLobbyStopClient ()
	{
		base.OnLobbyStopClient ();
		uiHandler.SetFirstPanel (true);
	
	}
	public override void OnLobbyStartHost ()
	{
		base.OnLobbyStartHost ();
		isHost = true;
//		FindObjectOfType<UIHandler> ().SetFirstPanel (false);
	}
	public override void OnLobbyStopHost ()
	{
		base.OnLobbyStopHost ();
		isHost = false;
	}
	public override void OnClientError (NetworkConnection conn, int errorCode)
	{
		base.OnClientError (conn, errorCode);
		isHost = false;
		uiHandler.SetFirstPanel (true);
	}


	void OnGUI(){
		if (isNetworkActive&&networkSceneName!=lobbyScene&&GUI.Button (new Rect (0, 30, 60, 30), "return")) {
			if (isHost)
				ServerReturnToLobby ();
			else
				SendReturnToLobby ();
		}
	}	
	}