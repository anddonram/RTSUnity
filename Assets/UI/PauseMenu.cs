using UnityEngine;
using System.Collections;
using RTS;
using UnityEngine.UI;
using UnityEngine.Networking;

public class PauseMenu : NetworkBehaviour
{

	private Player player;

	public GameObject menuPanel{ get; set; }

	void Awake ()
	{
		player = GetComponent< Player > ();
	}

	void Start ()
	{
		enabled = false;
	}

	void OnEnable ()
	{
		if (menuPanel)
			menuPanel.SetActive (true);
	}

	void OnDisable ()
	{
		if (menuPanel)
			menuPanel.SetActive (false);
	}

	[ClientCallback]
	void Update ()
	{
		if (Input.GetKeyDown (KeyCode.Escape)&&Time.timeScale==0)
			Resume ();
	}

	public void Resume ()
	{
		enabled = false;
		CmdResumeGame ();
	}

	public void ExitGame ()
	{
		CmdExit ();

	}

	[Command]
	void CmdResumeGame ()
	{
		RpcRestartAll ();
	}

	[ClientRpc]
	void RpcRestartAll ()
	{
		player.GetComponent< UserInput > ().enabled = true;
		Time.timeScale = 1.0f;
	}

	[Command]
	void CmdExit(){
		if (!player.IsDead ()) {
			player.Surrender ();
			RpcRestartAll ();
		}
	}
}
