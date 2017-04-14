using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
public class SynchroManager : NetworkBehaviour {


	[SyncVar (hook = "MoneyChange")]
	public int moneyLimit = 400;
	[SyncVar (hook = "WaterChange")]
	public int waterLimit = 250;
	[SyncVar (hook = "PopChange")]
	public int populationLimit = 4;
	[SyncVar (hook = "SceneChange")]
	public int lastScene = 0;
	[SyncVar]
	public string lastWin, lastWinner;

	Dropdown s;
	Slider pop, money, water;

	void MoneyChange (int arg0)
	{
		if (!money)
			money = GameObject.FindGameObjectWithTag ("Money").GetComponent<Slider> ();
		money.value = arg0 / 100;	
	}

	void WaterChange (int arg0)
	{
		if (!water)
			water = GameObject.FindGameObjectWithTag ("Water").GetComponent<Slider> ();
		water.value = arg0 / 50;
	}

	void PopChange (int arg0)
	{
		if (!pop)
			pop = GameObject.FindGameObjectWithTag ("Population").GetComponent<Slider> ();
		pop.value = arg0 / 5;
	}

	void SceneChange (int arg0)
	{
		if (!s)
			s = GameObject.FindGameObjectWithTag ("Scene").GetComponent<Dropdown> ();
		s.value = arg0;
	}
}
