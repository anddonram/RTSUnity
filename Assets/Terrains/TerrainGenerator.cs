using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
public class TerrainGenerator : MonoBehaviour
{
	public float tileSize = 10;
	public float xOrg;
	public float yOrg;

	public int iterations = 2;
	public int minHeight, maxHeight;
	[Range(0,2)]
	public int
		flatten = 1;
	[Range(0,1)]
	public float
		valleyFreq;
	private int xOffset, yOffset;
	[Range(0.1f,0.2f)]
	public float playerFieldSize;

	private Terrain terrain;

	void Awake ()
	{
		terrain = GetComponent<Terrain> ();
		//	GenerateHeights(GetComponent<Terrain> (),tileSize);
		//	GenerateHeights(GetComponent<Terrain> (),tileSize,xOrg,yOrg);
		GenerateHeights (terrain.terrainData, minHeight, maxHeight, iterations);
		for(int i=0;i<transform.childCount;i++){
			SetPosition(i);
			GenerateFieldSize(terrain,playerFieldSize,transform.GetChild(i).position);
		}
	}

	public void GenerateHeights (TerrainData terrain, float tileSize)
	{
		float width = terrain.heightmapWidth;
		float height = terrain.heightmapHeight;

		float[,] heights = new float[(int)width, (int)height];
		
		for (int i = 0; i < width; i++) {
			for (int k = 0; k < height; k++) {
				heights [i, k] = Mathf.PerlinNoise (((float)i / width) * tileSize, ((float)k / height) * tileSize) / 10.0f;
			}
		}
		
		terrain.SetHeights (0, 0, heights);
	}

	public void GenerateHeights (TerrainData terrain, float tileSize, float xOrg, float yOrg)
	{
		float width = terrain.heightmapWidth;
		float height = terrain.heightmapHeight;

		float[,] heights = new float[(int)width, (int)height];

		float y = 0.0F;
		while (y < height) {
			float x = 0.0F;
			while (x < width) {
				float xCoord = xOrg + x / width * tileSize;
				float yCoord = yOrg + y / height * tileSize;
				heights [(int)x, (int)y] = Mathf.PerlinNoise (xCoord, yCoord) * Mathf.PerlinNoise (yCoord, xCoord);
				x++;
			}
			y++;
		}
		terrain.SetHeights (0, 0, heights);
	}

	public void GenerateHeights (TerrainData terrain, int minHeight, int maxHeight, int iterations)
	{
		int width = terrain.heightmapWidth;
		int height = terrain.heightmapHeight;
		xOffset = Mathf.FloorToInt (width * 0.05f);
		yOffset = Mathf.FloorToInt (height * 0.05f);
		float[,] heights = new float[width, height];
		float max = 0, min = Mathf.Infinity;

		for (int it=0; it<iterations; it++) {

			int xCenter = Random.Range (xOffset, width - xOffset);
			int yCenter = Random.Range (yOffset, height - yOffset);
			int radius = Random.Range (minHeight, maxHeight);
			bool valley = Random.value <= valleyFreq;
			float sqrRadius = radius * radius;
			for (int i = -radius; i < radius; i++) {
				float sqrI = i * i;
				for (int k = -radius; k < radius; k++) {
					int indexX = xCenter + i;
					int indexY = yCenter + k;
					if (indexX < 0 || indexX >= width || indexY < 0 || indexY >= height)
						continue;
					float rise = Mathf.Max (0f, sqrRadius - (sqrI + (k * k)));
					if (valley)
						heights [indexX, indexY] -= rise;
					else
						heights [indexX, indexY] += rise;
					if (heights [indexX, indexY] > max) {
						max = heights [indexX, indexY];
					} else if (heights [indexX, indexY] < min) {
						min = heights [indexX, indexY];
					}
				}
			}
		}
		float deltaInverse = 1 / (max - min);
		for (int i = 0; i < width; i++) {
			for (int k = 0; k < height; k++) {
				heights [i, k] = (heights [i, k] - min) * deltaInverse;
				for (int f=0; f<flatten; f++)
					heights [i, k] *= heights [i, k];
			}
		}

		terrain.SetHeights (0, 0, heights);
	}
	public void GenerateFieldSize(Terrain terrain,int size,Vector3 position){
		TerrainData data = terrain.terrainData;
		int width = data.heightmapWidth;
		int height = data.heightmapHeight;
		
		int sizeWidth =Mathf.FloorToInt(2*size * width/data.size.x);
		int sizeHeight = Mathf.FloorToInt(2*size * height/data.size.z);
		
		Vector3 terrainPosition = (position - terrain.transform.position);
		int xPos =Mathf.FloorToInt( terrainPosition.x * width / data.size.x);
		int zPos =Mathf.FloorToInt( terrainPosition.z * height / data.size.z);
		float heightPoint = data.GetHeight (xPos, zPos)/data.size.y;
		
		float[,] heights = data.GetHeights(Mathf.Clamp (xPos - (sizeWidth / 2), 0, width - sizeWidth), Mathf.Clamp (zPos - (sizeHeight / 2), 0, height - sizeHeight),sizeWidth,sizeHeight);
		
		float maxDist = 2f/(sizeWidth * sizeWidth + sizeHeight * sizeHeight);
		for (int i = 0; i < sizeWidth; i++) {
			for (int k = 0; k < sizeHeight; k++) {
				heights [i, k] =Mathf.Lerp (heightPoint,heights[i,k],(((2*i-sizeWidth)*(2*i-sizeWidth)+(2*k-sizeHeight)*(2*k-sizeHeight))*maxDist));
			}
		}
		
		data.SetHeights (Mathf.Clamp (xPos - (sizeWidth / 2), 0, width - sizeWidth), Mathf.Clamp (zPos - (sizeHeight / 2), 0, height - sizeHeight), heights);

	}
	public void GenerateFieldSize (Terrain terrain, float fieldSize, Vector3 position)
	{
		TerrainData data = terrain.terrainData;
		int width = data.heightmapWidth;
		int height = data.heightmapHeight;

		int sizeWidth =Mathf.FloorToInt( fieldSize * width);
		int sizeHeight = Mathf.FloorToInt(fieldSize * height);

		Vector3 terrainPosition = (position - terrain.transform.position);
		int xPos =Mathf.FloorToInt( terrainPosition.x * width / data.size.x);
		int zPos =Mathf.FloorToInt( terrainPosition.z * height / data.size.z);
		float heightPoint = data.GetHeight (xPos, zPos)/data.size.y;

		float[,] heights = data.GetHeights(Mathf.Clamp (xPos - (sizeWidth / 2), 0, width - sizeWidth), Mathf.Clamp (zPos - (sizeHeight / 2), 0, height - sizeHeight),sizeWidth,sizeHeight);

		float maxDist = 2f/(sizeWidth * sizeWidth + sizeHeight * sizeHeight);
		for (int i = 0; i < sizeWidth; i++) {
			for (int k = 0; k < sizeHeight; k++) {
				heights [i, k] =Mathf.Lerp (heightPoint,heights[i,k],(((2*i-sizeWidth)*(2*i-sizeWidth)+(2*k-sizeHeight)*(2*k-sizeHeight))*maxDist));
			}
		}

		data.SetHeights (Mathf.Clamp (xPos - (sizeWidth / 2), 0, width - sizeWidth), Mathf.Clamp (zPos - (sizeHeight / 2), 0, height - sizeHeight), heights);
	}
	void SetPosition(int i){
	
		Vector3 center=(terrain.transform.position+terrain.terrainData.size)/2;
		float degrees=360*i/transform.childCount;
		Vector3 orientation=Quaternion.Euler (0, degrees, 0)*Vector3.right*terrain.terrainData.size.x*0.3f;
		Vector3 pos=center+orientation;
		pos.y = terrain.SampleHeight(pos);
		transform.GetChild(i).position=pos;
	}
}
