using UnityEngine;
using System.Collections;

public class LightCycle : MonoBehaviour {


	public Vector3 dayRotateSpeed;
	public Vector3 nightRotateSpeed;

	public float minMoonLight=0.4f;
	public float maxMoonLight=2.3f;
	public Light moonLight;
	public float nightTime=0.2f;
	Material skyMat;
	float skySpeed = 1;


	public float dayAtmosphereThickness = 0.4f;
	public float nightAtmosphereThickness = 0.87f;

	void Start () 
	{
		skyMat = RenderSettings.skybox;
	}

	void Update () {
		
		float dot =Vector3.Dot (transform.forward, Vector3.up);
		float i = ((maxMoonLight - minMoonLight) * Mathf.Clamp01((dot-nightTime)/(1-nightTime))) + minMoonLight;
		moonLight.intensity=i;


		dot =- Mathf.Clamp01 (dot);
		i = ((dayAtmosphereThickness - nightAtmosphereThickness) * dot) + nightAtmosphereThickness;
		RenderSettings.ambientIntensity = i;
		skyMat.SetFloat ("_AtmosphereThickness", i);
	
		if (dot > 0) 
			transform.Rotate (dayRotateSpeed * Time.deltaTime * skySpeed);
		else
			transform.Rotate (nightRotateSpeed * Time.deltaTime * skySpeed);
	

		if (Input.GetKeyDown (KeyCode.Q)) Time.timeScale *= 0.5f;
		if (Input.GetKeyDown (KeyCode.E)) Time.timeScale *= 2f;
	}
}
