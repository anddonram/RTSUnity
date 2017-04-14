using UnityEngine;
using System.Collections;
using RTS;
using UnityEngine.Networking;
using System.Collections.Generic;

public class AITrain : NetworkBehaviour
{

	public AIHandler aihandler;
	private AIResource airesource;
	private AIBuilding aibuilding;
	private BackPropagationAlgorithm bpa;

	public int unitToTrain = 7;
	private List<TrainingField> fields;
	public bool auto=false;
	[ServerCallback]
	void Start ()
	{
		InvokeRepeating ("Recalculate", 5 + Random.value * 3, 5);
		airesource = aihandler.airesource;
		aibuilding = aihandler.aibuilding;
		bpa = aihandler.bpa;

		fields = new List<TrainingField> ();
	}

	[Server]
	void Recalculate ()
	{

		if (NeedTraining ()) { 
			if (bpa&&auto)
				unitToTrain = bpa.GetResults ();
			ChooseTraining ();
		}
	}


	bool NeedTraining ()
	{
		return aihandler.GetMinHarvesters () < aihandler.playerList.harvesters.Count;
	}

	void ChooseTraining ()
	{
		fields.Clear ();
		fields.AddRange (aihandler.playerList.fields);
		if (fields.Count == 0) {
			aibuilding.BuildTrainingField ();
		} else {
			if (fields.Count < 4) {
				aibuilding.BuildTrainingField ();
			}
			HashSet<BaseUnit> units = aihandler.playerList.baseUnits;
			int keep = aihandler.playerList.harvesters.Count;
			foreach (Unit u in units) {
				
				if (u.value > 6 || (u.state == WOState.Attacking && u.hitPoints >= u.maxHitPoints * 0.4f))
					continue;
				if (u is Harvester) {
					if (u.state == WOState.Waiting)
						keep--;
					if (aihandler.GetMinHarvesters () >= keep)
						continue;
				}
				int range = -1;
				if (bpa) {
					if (u.value >= unitToTrain)
						continue;
					int canBeTrained = unitToTrain - u.value;
				
					if ((canBeTrained & u.value) == 0) {
						for (int i = 0; i < fields.Count; i++) {
							if ((1 <<fields [i].value & canBeTrained) != 0) {
					
								range = i;
								break;
							}
						}
						if (range == -1) {

							aibuilding.AskForTrainingFields (canBeTrained);
						}
				
					}
				}
				if (range == -1) {
					range = Random.Range (0, fields.Count);
				}

				TrainingField field = fields [range];
			
				WorldObject wo;
				if (field.value == 5) {
					if (u.value == 0 || u.specialActivated)
						continue;
					wo = u;
				} else {
					if ((u.value & (1 << field.value)) != 0)
						continue;
					GameObject gwo = field.getOwner ().GetUnit (field.value + (1 << u.value));
					if (!gwo)
						continue;
					wo = gwo.GetComponent<WorldObject> ();
				}
				if (!aihandler.HasResources (wo, ResourceType.Money)) {
					airesource.AllocateMoney (wo.moneyCost);
				} 
				if (!aihandler.HasResources (wo, ResourceType.Water)) {
					airesource.AllocateWater (wo.waterCost);
				} else if (aihandler.HasResources (wo, ResourceType.Money)) {
					u.MouseClick (field.gameObject, field.transform.position, aihandler.playerId, true);
					if (u.value == 0) {
						keep--;
					}
					
				}
			}
		
		}

	}

}
