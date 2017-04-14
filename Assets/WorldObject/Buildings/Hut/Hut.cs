using UnityEngine;
using System.Collections;
using RTS;
using System.Collections.Generic;
using UnityEngine.Networking;
public class Hut : Building {

	public float populationSpeed;
	private bool harvest=true;
	private bool reharv=false;
	[ServerCallback]
	protected override void Update () {
		base.Update ();
			if (buildQueue.Count == 0
			&& owner.GetResource (ResourceType.Population) < owner.GetResourceLimit (ResourceType.Population)) {
			maxBuildProgress=2+owner.GetResource (ResourceType.Population)*populationSpeed;
			if (harvest) {
				CreateUnit (actions [0]);
			} else {
				CreateUnit (actions [1]);
			}
			bool aux = harvest;
			harvest = (reharv&&!harvest)||(harvest&&!reharv);
			reharv = aux;
		}
	}
	public override void Sell ()
	{
		if( owner.playerList.huts.Count>1)
			base.Sell();
	}
	public override string[] GetActions()
	{
		return new string[0];
	}

	[ServerCallback]
	protected override void OnDestroy ()
	{
		base.OnDestroy ();
		if (owner) {
			owner.IncreaseResourceLimit (ResourceType.Money, -50);
			owner.IncreaseResourceLimit (ResourceType.Water, -25);
		}
	}
 }
