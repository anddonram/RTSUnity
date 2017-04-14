using UnityEngine;
using System.Collections;
using RTS;
namespace Prototype.NetworkLobby{
public class CustomHook : LobbyHook {

		public override void OnLobbyServerSceneLoadedForPlayer (UnityEngine.Networking.NetworkManager manager, GameObject lobbyPlayer, GameObject gamePlayer)
		{
			LobbyPlayer cc = lobbyPlayer.GetComponent<LobbyPlayer> ();
			Player player = gamePlayer.GetComponent<Player> ();

			player.username = cc.playerName;
			player.spectator = cc.spectator;

			player.teamColor = cc.playerColor;
			player.clan = cc.playerClan;
			player.techTree = (TechTree)Instantiate (ResourceManager.GetTechTree (player.clan), transform.position, transform.rotation);
			player.techTree.player = player;


//			player.startMoney;
//			player.startWater;
//
//			player.startMoneyLimit = Control.moneyLimit;
//			player.startWaterLimit = Control.waterLimit;
//			player.populationLimit = Control.populationLimit;

		}
}
}