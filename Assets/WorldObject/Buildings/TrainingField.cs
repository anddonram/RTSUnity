using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;
using UnityEngine.Networking;
public class TrainingField : Building {

	protected Unit training;

	protected override void ProcessBuildQueue ()
	{
		if(training) {
			if(currentBuildProgress==0){
				WorldObject wo=owner.GetUnit(training.value+(1<<value)).GetComponent<WorldObject>();
			if	(owner.GetResource (ResourceType.Money) >= wo.moneyCost
			   && owner.GetResource (ResourceType.Water) >= wo.waterCost){
				Consume(wo.woName);
				training.gameObject.SetActive(false);
				owner.RpcSetActive(training.gameObject,false);
				}
			}

			if(!training.gameObject.activeSelf)
			currentBuildProgress+=Time.deltaTime;
			
			if(currentBuildProgress>=maxBuildProgress){
				owner.AddUnit(training.value+(1<<value),spawnPoint.position,transform.rotation,rallyPoint,nextWo,running);
				NetworkServer.Destroy(training.gameObject);
				currentBuildProgress=0;
			}
		}
	}
	
	public override void Enter(Unit b){
		base.Enter (b);
		if (!training)
		if ((b.value & (1 << value)) == 0) {
			training = b;
			b.state = RTS.WOState.Entering;
		} else
			b.StopActions ();
	}

	public override void Exit(){
		base.Exit ();
		if (training) {
			training.StopActions ();
			training.gameObject.SetActive (true);
			owner.RpcSetActive(training.gameObject,true);
			RecoverResources (training.value+(1<<value));
			currentBuildProgress=0;
			training = null;
		}
	}
	[Server]
	public override void CreateUnit (string unitName)
	{
		WorldObject wo = owner.GetUnit (unitName).GetComponent<WorldObject> ();
		if (owner.GetResource (ResourceType.Money) >= wo.moneyCost
			&& owner.GetResource (ResourceType.Water) >= wo.waterCost
			&& owner.GetResource (ResourceType.Population) <owner.populationLimit) {
			Consume (unitName);
			owner.AddUnitFromString (unitName, spawnPoint.position, transform.rotation, rallyPoint, nextWo, running);
		}
	}
	protected override void OnMouseEnter ()
	{ 
		if (owner && owner.isLocalPlayer && owner.SelectedObject) {
				BaseUnit unit = owner.SelectedObject as BaseUnit;
			if (unit && unit.IsOwnedBy (ownerId)) {
					GameObject g = owner.GetUnit (unit.value + (1 << value));
					if (g) {
						owner.ui.UpdateResourcePanel (g.GetComponent<BaseUnit> ());
					}
				}
			}
			base.OnMouseEnter ();
		
	}
	protected override void OnMouseExit ()
	{
		if (owner&&owner.isLocalPlayer) {
			owner.ui.ClearResourcePanel ();
		}
		base.OnMouseExit ();
	}
}
