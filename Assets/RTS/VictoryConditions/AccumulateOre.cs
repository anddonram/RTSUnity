using UnityEngine;
using System.Collections;
using RTS;
public class AccumulateOre : VictoryCondition {
	public int amount;
	public override string GetDescription (){
		return "muerto por exceso de money";
	}
	public override bool PlayerMeetsCondition (Player p){
		return p && !p.IsDead () && p.GetResource (ResourceType.Money) >= amount;
	}
}
