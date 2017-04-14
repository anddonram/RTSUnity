using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour
{

	private CustomLobby customLobby;
	public GameObject firstPanel, secondPanel, thirdPanel;

	void Start ()
	{
		customLobby = GetComponent<CustomLobby> ();

	}

	public void SetupServer ()
	{
		customLobby.StartServer ();
		//	SetFirstPanel (false);
	}

	public void SetupClient ()
	{
		if (customLobby.isNetworkActive)
			customLobby.StopClient ();
		customLobby.StartClient ();
	}

	public void SetupHost ()
	{
		if (customLobby.isNetworkActive)
			customLobby.StopHost ();
		customLobby.StartHost ();
	}

	public Text readyButton;

	public void Ready ()
	{
		foreach (NetworkLobbyPlayer p in customLobby.lobbySlots)
			if (p && p.isLocalPlayer) {
				if (!p.readyToBegin) {
					p.SendReadyToBeginMessage ();
					readyButton.text = "Not Ready";
				} else {
					p.SendNotReadyToBeginMessage ();
					readyButton.text = "Ready";
				}
				break;
			}
	}

	public void Stop ()
	{
		if (customLobby.isHost) {
			customLobby.StopHost ();
		}else {
			customLobby.StopClient ();
		}

	}

	public void ChangeIP (string ip)
	{
		customLobby.networkAddress = ip;
	
	}

	public void ChangePort (string port)
	{
		int res;
		if (int.TryParse (port, out res)) {
			customLobby.networkPort = res;
		}
	}

	public Text popText;

	public void ChangePop (float val)
	{
		popText.text = (val * 5).ToString ();
	}

	public Text moneyText;

	public void ChangeMoney (float val)
	{
		moneyText.text = (val * 100).ToString ();
	}

	public Text waterText;

	public void ChangeWater (float val)
	{
		waterText.text = (val * 50).ToString ();
	}


	public void SetFirstPanel (bool first)
	{
		firstPanel.SetActive (first);
		secondPanel.SetActive (!first);
		thirdPanel.SetActive (!first);
	}

	public void SetStopPanel ()
	{
		firstPanel.SetActive (false);
		secondPanel.SetActive (true);
		thirdPanel.SetActive (false);
	}

	public void SetUI (bool res)
	{
		GetComponent<Canvas> ().enabled = res;
		GetComponent<CanvasScaler> ().enabled = res;
		GetComponent<GraphicRaycaster> ().enabled = res;
	}
}
