using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class FogPixel : MonoBehaviour
{
	public Transform units;
	public float refreshTime = 1;
	private Texture2D texture;
	private List<Vector3> before;

	void Awake ()
	{
		texture = GetComponent<Renderer> ().sharedMaterial.mainTexture as Texture2D;
		before = new List<Vector3> ();
		Reset ();
	
	}

	public void StartFog ()
	{
		InvokeRepeating ("Recalculate", 0.5f, refreshTime);
	}

	void Recalculate ()
	{
		RaycastHit hit;
		Transform u;
		Color[] colors;
		Vector3 coord;
		for (int i = 0; i < before.Count; i++) {
			coord = before [i];
			colors = GetBlank ((int)coord.z);
			texture.SetPixels ((int)coord.x, (int)coord.y, (int)(2 * coord.z), (int)(2 * coord.z), colors);
		}
		before.Clear ();
		int le = units.childCount;
		for (int i = 0; i < le; i++) {
			u = units.GetChild (i).transform;
			Vector3 pos = u.position;
			pos.y = 0;
			if (Physics.Raycast (pos, Vector3.up, out hit, 1 << 9)) {

				int radius = u.GetComponent<CustomProximityChecker> ().visRange;
				Vector2 hitCoordinates = hit.textureCoord * texture.height;
				int x = (int)hitCoordinates.x - radius;
				int y = (int)hitCoordinates.y - radius;
				x = Mathf.Clamp (x, 0, texture.height - 2 * radius);
				y = Mathf.Clamp (y, 0, texture.height - 2 * radius);
				colors = GetCircle (x,y,radius);
				texture.SetPixels (x, y, 2 * radius, 2 * radius, colors);
				before.Add (new Vector3(x,y,radius));
			}
		}
		texture.Apply ();
	}
	Color[] GetPartialCircle(int centreX, int centreY, int radius){
		int realx = 2*radius;
		int realy = 2*radius;
		int startX = 0;
		int startY = 0;
		if (centreX - radius < 0) {
			realx = radius + centreX;
			startX = radius - centreX;
		} else if (centreX + radius>texture.width) {
			realx = radius - centreX +texture.width;
		}
		if (centreY - radius < 0) {
			realy = radius + centreY;
			startY= radius- centreY;
		} else if (centreY + radius>texture.height) {
			realy = radius - centreY +texture.height;
		}
		Color[] pixels = new Color[realx * realy];
		for (int i = 0; i < realx; i++) {
			for (int j = 0; j < realy; j++) {
				int index = realy * i + j;
				int xS = i + startX - radius;
				int yS = j + startY - radius;
				if (xS * xS + yS * yS < radius * radius)
					pixels [index] = Color.clear;
				else
					pixels [index] = Color.white;
			}
		}
		return pixels;
	}
	 Color[] GetCircle (int xPos,int yPos,int radius)
	{
		
		int diameter = 2 * radius;
		Color[] pixels = texture.GetPixels(xPos,yPos,diameter,diameter);
		for (int x = -radius; x < radius; x++) {
			for (int y = -radius; y < radius; y++) {
				int index = diameter * (x + radius) + (y + radius);
				if (x * x + y * y < radius * radius)
					pixels [index] *= Color.clear;
				else
					pixels [index] *= Color.white;
			}
		}
		return pixels;
	}
	 Color[] GetBlank (int radius)
	{
		Color[] pixels = new Color[4*radius*radius];
		for (int x = 0; x < pixels.Length; x++) {
				pixels [x] = Color.white;
		}
		return pixels;
	}


	public void Reset ()
	{
//		Color[] res= GetPartialCircle (0,512,64);
//		int le = (int)Mathf.Sqrt (res.Length);
//		texture.SetPixels(0,512,64,64, res);
		texture.SetPixels (GetBlank(texture.width/2));
		texture.Apply ();
	}
}
