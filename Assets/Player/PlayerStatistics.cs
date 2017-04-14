using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using RTS;
public class PlayerStatistics : MonoBehaviour
{

	/**
	 * 0 is money
	 * 1 is water
	 */
	public float[] resourcesGathered, resourcesSpent;
	/**
	 * world object value is index
	 */
	public float[] buildingsBuilt, buildingsLost, unitsTrained, unitsLost, unitsSpecialized;

	void Awake ()
	{
		Player player = GetComponent<Player> ();
		resourcesGathered = new float[2];
		resourcesSpent = new float[2];
		for (int i = 0; i < resourcesGathered.Length; i++) {
			resourcesGathered [i] = 0;
			resourcesSpent [i] = 0;
		}
		buildingsBuilt = new float[ player.GetBuildingListCount ()];
		buildingsLost = new float[ player.GetBuildingListCount ()];

		for (int i = 0; i < buildingsBuilt.Length; i++) {
			buildingsBuilt [i] = 0;
			buildingsLost [i] = 0;
		}
		unitsTrained = new float[player.GetUnitListCount ()];
		unitsLost = new float[player.GetUnitListCount ()];
		unitsSpecialized = new float[player.GetUnitListCount ()];

		for (int i = 0; i < unitsTrained.Length; i++) {
			unitsTrained [i] = 0;
			unitsLost [i] = 0;
			unitsSpecialized [i] = 0;
		}

	}
	public void AddCreated(WorldObject wo){
		if (wo is Unit) {
			unitsTrained [wo.value]++;
		} else if (wo is Building) {
			buildingsBuilt [wo.value]++;
		}
	}
	public void AddDestroyed(WorldObject wo){
		if (wo is Unit) {
			unitsLost [wo.value]++;
		} else if (wo is Building) {
			buildingsLost [wo.value]++;
		}
	}
	public void AddSpecial(Unit u){
		unitsSpecialized [u.value]++;
	}
	public void AddResource(ResourceType type, float amount){
		if (amount > 0) {
			if (type== ResourceType.Money) {
				resourcesGathered [0] += amount;
			} else if (type == ResourceType.Water) {
				resourcesGathered [1] += amount;
			}
		} else {
			if (type == ResourceType.Money) {
				resourcesSpent [0] -= amount;
			} else if (type == ResourceType.Water) {
				resourcesSpent [1] -= amount;
			}
		}
	}
}
