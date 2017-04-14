using UnityEngine;
using System.Collections;
using RTS;
public class GenericObjectList : MonoBehaviour {

	public Texture2D[] cursors;
	public GameObject canvas;
	public GameObject fog;
	public Material allowedMaterial;
	void Awake ()
	{
		ResourceManager.SetGenericObjectList (this);
	}

}
