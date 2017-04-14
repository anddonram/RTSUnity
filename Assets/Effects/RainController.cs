using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class RainController : NetworkBehaviour
{

	private ParticleSystem par;

	void Awake ()
	{
		par = GetComponent<ParticleSystem> ();
	}

	public bool IsRaining ()
	{		
		return par != null && par.isPlaying;
	}

	public void Rain ()
	{
		if (!IsRaining ())
			par.Play ();
	
	}


		

}
