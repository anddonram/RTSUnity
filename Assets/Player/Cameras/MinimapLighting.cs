using UnityEngine;
using System.Collections;

public class MinimapLighting : MonoBehaviour {


	private Camera minimap;
	public Shader shader;
	void Start () {
		minimap = GetComponent<Camera> ();
		minimap.SetReplacementShader (shader,string.Empty);	
	}

}
