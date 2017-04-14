using UnityEngine;
using System.Collections;
using RTS;
using UnityEngine.Networking;
using System.Collections.Generic;

public class AIAttack : NetworkBehaviour
{
	public AIHandler aihandler;

	private List<Unit> troops;
	private List<WorldObject> onRange;
	private List<Unit> explorers;
	private List<Building> enemyBuildings;
	private List<Unit> toRemove;

	private PlayerStatistics statistics;
	private BackPropagationAlgorithm bpa;

	private Vector3 terrMin, terrMax;
	private float[] lastUnits,lastEnemyUnits;

	[ServerCallback]
	void Start ()
	{
		terrMin = aihandler.terrain.transform.position;
		terrMax = terrMin + aihandler.terrain.terrainData.size;
	
		troops = new List<Unit> ();
		explorers = new List<Unit> ();
		onRange = new List<WorldObject> ();
		toRemove = new List<Unit> ();
		enemyBuildings = new List<Building> ();
		InvokeRepeating ("Recalculate", 10 + Random.value * 3, 5);

		bpa = aihandler.bpa;
		if (bpa) {
			lastUnits=new float[bpa.numOutput];
			lastEnemyUnits=new float[bpa.numInput];
			statistics = aihandler.player.statistics;
		}
	}

	[Server]
	void Recalculate ()
	{

		if (bpa) {
			bpa.ResetInputs ();
			bpa.ResetOutputs ();
		}
		troops.AddRange (aihandler.playerList.units);
		troops.RemoveAll (u => u is Harvester || u is Worker);
		onRange.Clear ();
		if (EnemiesNear ()) {
			Attack ();

		} else if (!Exploring ()) {
			ChooseExplorers ();
			if (explorers.Count > 0)
				Explore ();			
		}
	
		if (bpa&& HasTrainingData ()) {
			bpa.Train();
			bpa.inputs = lastEnemyUnits;
		}
		troops.Clear ();
	}

	bool EnemiesNear ()
	{
		bool res = false;

		foreach (Unit u in aihandler.playerList.units) {
			Collider[] hits = Physics.OverlapSphere (u.transform.position, u.prox.visRange, 1);
			WorldObject wo;
			foreach (Collider hit in hits) {
				wo = hit.transform.parent.GetComponent<WorldObject> ();
				if (wo && !(wo is Other || wo is Resource) && !wo.IsOwnedBy (aihandler.playerId) && !onRange.Contains (wo)) {
					onRange.Add (wo);
					Building b = wo as Building;
					if (b && !enemyBuildings.Contains (b)) {
						enemyBuildings.Add (b);
					} else if (bpa && wo is Unit && wo.value>0 && wo.value<8) {
						bpa.inputs [wo.value - 1]++;
					}
					res = true;
				}
			}

		}
		return res;
	}

	void Attack ()
	{
		if (troops.Count > 0) {
			int army = troops.Count /2;
			int i = 0;
			WorldObject wo;
			foreach (Unit u in troops) {
				if (!u.CanAttack () || u.state == WOState.Attacking||u.state==WOState.Waiting)
					continue;
				if (i < army && army >= 4&&enemyBuildings.Count>0) {
					wo = enemyBuildings [0];
					i++;
				} else {
					int range = Random.Range (0, onRange.Count);
					wo = onRange [range];
				}
				if (wo)
					u.MouseClick (wo.gameObject, wo.transform.position, aihandler.playerId, true);
				else {
					onRange.Remove (wo);
					enemyBuildings.RemoveAt (0);
				}
			}
		}
	}

	bool Exploring ()
	{
		toRemove.Clear();
		explorers.RemoveAll (u => u == null);
		foreach (Unit u in explorers) {
			if (WorkManager.VectorEquals (u.transform.position, u.GetDestination (), u.prox.visRange)) {
				toRemove.Add (u);
			}
		}
		foreach (Unit u in toRemove) {
			Vector2 v = Random.insideUnitCircle * 10;

			Vector3 move = transform.position + new Vector3 (v.x, 0, v.y);
			u.MouseClick (aihandler.terrain.gameObject, move, aihandler.playerId, false);
			explorers.Remove (u);
		}

		return explorers.Count > 0;
	}

	void ChooseExplorers ()
	{
		if (explorers.Count >= GetMaxExplorers ())
			return;
		foreach (Unit u in troops) {
			if (u.state == WOState.Nothing && !explorers.Contains (u)) {
				explorers.Add (u);
				if (explorers.Count >= GetMaxExplorers ())
					break;
			}
		}

	}

	void Explore ()
	{
		
		foreach (Unit u in explorers) {
			float x = Random.Range (terrMin.x, terrMax.x);
			float z = Random.Range (terrMin.z, terrMax.z);
			Vector3 move = new Vector3 (x, 0, z);
			u.MouseClick (aihandler.terrain.gameObject, move, aihandler.playerId, false);
		}
	}

	public int GetMaxExplorers ()
	{
		int res = aihandler.player.GetResource (ResourceType.Population) /5;

		return Mathf.Max (1, res);
	}
	bool HasTrainingData(){
		bool resin = false;
		bool resout = false;
		float[] currentUnits = new float[bpa.numOutput];
		float[] currentEnemyUnits=(float[])bpa.inputs.Clone();

		for (int i = 1; i < 8; i++) {
			currentUnits[i-1]=-statistics.unitsLost [i - 1];
		}
		for (int i = 0; i < currentUnits.Length; i++) {
			bpa.outputs [i] = currentUnits [i] - lastUnits [i];
			bpa.inputs [i] = currentEnemyUnits [i] - lastEnemyUnits [i];
			resin = resin || bpa.outputs [i] != 0;
			resout = resout || bpa.inputs [i] != 0;
		}
		if(resin&&resout)
			lastUnits=currentUnits;
		lastEnemyUnits = currentEnemyUnits;
		return resin && resout;
	}
}
