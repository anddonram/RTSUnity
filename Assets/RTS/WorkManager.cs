using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using RTS;

public class WorkManager : MonoBehaviour {


	public static Vector3 FindHitPoint() {
		if (EventSystem.current.IsPointerOverGameObject ())
			return ResourceManager.InvalidPosition;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if(Physics.Raycast(ray, out hit,200,1<<8)) return hit.point;
		return ResourceManager.InvalidPosition;
	}

	public static bool VectorEquals(Vector3 v1,Vector3 v2,float precision){
		return (v1 - v2).sqrMagnitude < precision;
	}
}
