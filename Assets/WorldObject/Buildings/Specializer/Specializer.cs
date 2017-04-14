using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using RTS;
public class Specializer : TrainingField {

	protected override void ProcessBuildQueue ()
	{
		if(training) {
			if(currentBuildProgress==0){
				if	(owner.GetResource (ResourceType.Money) >= training.moneyCost
				    && owner.GetResource (ResourceType.Water) >= training.waterCost){
					Consume(training.woName);
					training.gameObject.SetActive(false);
					owner.RpcSetActive(training.gameObject,false);
				}
			}
			
			if(!training.gameObject.activeSelf)
				currentBuildProgress+=Time.deltaTime;
			
			if(currentBuildProgress>=maxBuildProgress){
				training.specialActivated=true;
				training.StopActions ();
				training.gameObject.SetActive (true);
				owner.statistics.AddSpecial (training);
				owner.RpcSetActive(training.gameObject,true);
				training=null;
				currentBuildProgress=0;
			}
		}
	}

	protected override void OnMouseEnter ()
	{ 
		if (owner && owner.isLocalPlayer && owner.SelectedObject) {
			BaseUnit unit = owner.SelectedObject as BaseUnit;
			if (unit && unit.IsOwnedBy (ownerId)) {
					owner.ui.UpdateResourcePanel (unit);
			}
		}
		base.OnMouseEnter ();

	}
	public override void Enter (Unit b)
	{
		if (!(b is Harvester)&&!b.specialActivated) {
			base.Enter (b);
		}
	}
}
