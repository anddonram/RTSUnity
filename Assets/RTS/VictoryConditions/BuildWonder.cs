using UnityEngine;
using System.Collections;

public class BuildWonder : VictoryCondition {

	public override string GetDescription (){
		return "victoria por super maravilla";
	}
	public override bool PlayerMeetsCondition (Player p){
		bool res = p && !p.IsDead ();
		if (res) {
			Wonder w=	p.buildings.GetComponentInChildren<Wonder> ();
			res = w && !w.UnderConstruction ();
		}
			
		return res;
	}
}
