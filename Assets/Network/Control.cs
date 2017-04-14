using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using RTS;

public class Control : NetworkLobbyPlayer
{
	[SyncVar]
	public Color color = Color.black;
	[SyncVar]
	public string username = string.Empty;
	[SyncVar]
	public Clan clan = Clan.Sig;
	[SyncVar]
	public bool isSpectator = false, isMock = false;
	Dropdown d, s;
	//Slider pop,money,water;

	void ChangeName (string arg0)
	{
		CmdChangeName (arg0);
	}

	void ChangeClan (int arg0)
	{
		CmdChangeClan (arg0);
	}

	void ChangeSpectator (bool spec)
	{
		CmdChangeSpectator (spec);
	}

	void ChangeMock (bool spec)
	{
		CmdChangeMock (spec);
	}

	void ChangeColor (int arg0)
	{
		if (ColorUtility.TryParseHtmlString (d.options [arg0].text, out color)) {
			d.targetGraphic.color = color;
			CmdChangeColor (color);
		}
	}

	void ChangeScene (int arg0)
	{
		if (isServer)
			CmdChangeScene (arg0);
	}

	void ChangePopulation (float arg0)
	{
		if (isServer)
			CmdChangePopulation (arg0);
	}

	void ChangeMoney (float arg0)
	{
		if (isServer)
			CmdChangeMoney (arg0);
	}

	void ChangeWater (float arg0)
	{
		if (isServer)
			CmdChangeWater (arg0);
	}

	[Command]
	void CmdChangeScene (int arg0)
	{
		CustomLobby.single.playScene = s.options [arg0].text;
		CustomLobby.single.synchroManager.lastScene = arg0;
//		RpcChangeScene (arg0);
	}

	[Command]
	void CmdChangePopulation (float arg0)
	{
		CustomLobby.single.synchroManager.populationLimit = (int)arg0 * 5;
//		RpcChangePopulation (arg0);
	}

	[Command]
	void CmdChangeMoney (float arg0)
	{
		CustomLobby.single.synchroManager.moneyLimit = (int)arg0 * 100;
//		RpcChangeMoney (arg0);
	}

	[Command]
	void CmdChangeWater (float arg0)
	{
		CustomLobby.single.synchroManager.waterLimit = (int)arg0 * 50;
//		RpcChangeWater(arg0);
	}
	//	[ClientRpc]
	//	void RpcChangeScene (int arg0)
	//	{
	//		s.value=arg0;
	//	}
	//	[ClientRpc]
	//	void RpcChangePopulation (float arg0)
	//	{
	//		pop.value = arg0;
	//	}
	//
	//	[ClientRpc]
	//	void RpcChangeMoney (float arg0)
	//	{
	//		money.value=arg0;
	//	}
	//
	//	[ClientRpc]
	//	void RpcChangeWater (float arg0)
	//	{
	//		water.value=arg0;
	//	}
	[Command]
	void CmdChangeName (string name)
	{
		username = name;
	}

	[Command]
	void CmdChangeSpectator (bool spec)
	{
		isSpectator = spec;
	}

	[Command]
	void CmdChangeMock (bool mock)
	{
		isMock = mock;
	}

	[Command]
	void CmdChangeColor (Color color)
	{
		this.color = color;
	}

	[Command]
	void CmdChangeClan (int arg0)
	{
		this.clan = (Clan)arg0;
	}

	public void Restart ()
	{
		
		InputField i = GameObject.FindGameObjectWithTag ("Username").GetComponent<InputField> ();
		i.onEndEdit.AddListener (ChangeName);
	//	i.text = username;

		d = GameObject.FindGameObjectWithTag ("Color").GetComponent<Dropdown> ();
		d.onValueChanged.AddListener (ChangeColor);
	//	d.value=0;

		Dropdown e = GameObject.FindGameObjectWithTag ("Clan").GetComponent<Dropdown> ();
		e.onValueChanged.AddListener (ChangeClan);
	//	e.value=(int)clan;

		Toggle spectator = GameObject.FindGameObjectWithTag ("Spectator").GetComponent<Toggle> ();
		spectator.onValueChanged.AddListener (ChangeSpectator);
	//	spectator.isOn = isSpectator;

		Toggle mock = GameObject.FindGameObjectWithTag ("Mock").GetComponent<Toggle> ();
		mock.onValueChanged.AddListener (ChangeMock);
	//	mock.isOn = isMock;

		Slider pop = GameObject.FindGameObjectWithTag ("Population").GetComponent<Slider> ();
		Slider water = GameObject.FindGameObjectWithTag ("Water").GetComponent<Slider> ();
		Slider money = GameObject.FindGameObjectWithTag ("Money").GetComponent<Slider> ();
		s = GameObject.FindGameObjectWithTag ("Scene").GetComponent<Dropdown> ();

		if (CustomLobby.single.isHost) {

			pop.interactable = true;
			water.interactable = true;
			money.interactable = true;
			s.interactable = true;	
			pop.onValueChanged.AddListener (ChangePopulation);		
			water.onValueChanged.AddListener (ChangeWater);
			money.onValueChanged.AddListener (ChangeMoney);
			s.onValueChanged.AddListener (ChangeScene);
		} else {
			pop.interactable = false;
			water.interactable = false;
			money.interactable = false;
			s.interactable = false;
		}
	}

//	public override void OnClientEnterLobby ()
//	{
//		
//		if (isLocalPlayer||CustomLobby.single.lobbyScene==CustomLobby.networkSceneName) {
//			
//			base.OnClientEnterLobby ();
//			FindObjectOfType<UIHandler> ().SetFirstPanel (false);
//			Restart ();
//
//		}
//	}

	public override void OnStartLocalPlayer ()
	{

		if (CustomLobby.single.lobbyScene==CustomLobby.networkSceneName||CustomLobby.networkSceneName==string.Empty) {

			base.OnStartLocalPlayer ();
			CustomLobby.single.uiHandler.SetFirstPanel (false);
			GameObject.FindGameObjectWithTag ("LastWin").GetComponent<Text> ().text	=	CustomLobby.single.synchroManager.lastWin;
			GameObject.FindGameObjectWithTag ("LastWinner").GetComponent<Text> ().text	=	CustomLobby.single.synchroManager.lastWinner;

			Restart ();
		}
	}
}
