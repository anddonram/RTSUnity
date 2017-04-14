using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
public class OreDeposit : Resource {
	private int numBlocks;
	private RainController rain;
	public float regenerateTime=2;
	private Renderer[] renderers;
	[ServerCallback]
	protected override void Start(){
		base.Start ();
		renderers = GetComponentsInChildren<Renderer> (true);
		rain = FindObjectOfType<RainController> ();
		numBlocks=renderers.Length;
		type = RTS.ResourceType.Money;
		InvokeRepeating ("Regenerate", 5, regenerateTime);
	}

	void Regenerate(){
		if (rain.IsRaining ()) {
			Remove (-2);
		} else {
			Remove (-1);
		}
 	}
	public override void Remove(int amount){
		base.Remove (amount);

		int numBlocksToShow = numBlocks*hitPoints / maxHitPoints;
			for(int i=0;i<numBlocksToShow;i++)
			renderers[i].enabled=true;
			
			for(int i=numBlocksToShow;i<numBlocks;i++)
			renderers[i].enabled=false;

	}
}
