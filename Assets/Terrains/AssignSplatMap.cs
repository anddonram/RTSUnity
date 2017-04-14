using UnityEngine;
using System.Collections;
using System.Linq; // used for Sum of array

public class AssignSplatMap : MonoBehaviour {
	public float firstTextureInfluence=0.8f;	
	void Start () {

		// Get a reference to the terrain data
		TerrainData terrainData =GetComponent<Terrain>().terrainData;
		
		// Splatmap data is stored internally as a 3d array of floats, so declare a new empty array ready for your custom splatmap data:
		float[, ,] splatmapData = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];
		
		for (int y = 0; y < terrainData.alphamapHeight; y++)
		{
			for (int x = 0; x < terrainData.alphamapWidth; x++)
			{
				// Normalise x/y coordinates to range 0-1 
				float y_01 = (float)y/(float)terrainData.alphamapHeight;
				float x_01 = (float)x/(float)terrainData.alphamapWidth;
				
				// Sample the height at this location (note GetHeight expects int coordinates corresponding to locations in the heightmap array)
//				float height = terrainData.GetHeight(Mathf.RoundToInt(y_01 * terrainData.heightmapHeight),Mathf.RoundToInt(x_01 * terrainData.heightmapWidth) );

				// Calculate the steepness of the terrain
				float steepness = terrainData.GetSteepness(y_01,x_01);
				
				// Setup an array to record the mix of texture weights at this point
				float[] splatWeights = new float[terrainData.alphamapLayers];
				
				// CHANGE THE RULES BELOW TO SET THE WEIGHTS OF EACH TEXTURE ON WHATEVER RULES YOU WANT
				
				// Texture[0] has constant influence
				splatWeights[0] = firstTextureInfluence;

				// Texture[2] stronger on flatter terrain
				// Note "steepness" is unbounded, so we "normalise" it by dividing by the extent of heightmap height and scale factor
				// Subtract result from 1.0 to give greater weighting to flat surfaces
				splatWeights[2] = Mathf.Clamp01(steepness*steepness/terrainData.heightmapHeight);
				splatWeights[1] = 1.0f - splatWeights[2];
			
				// Sum of all textures weights must add to 1, so calculate normalization factor from sum of weights
				float z = splatWeights.Sum();
				
				// Loop through each terrain texture
				for(int i = 0; i<terrainData.alphamapLayers; i++){
					
					// Normalize so that sum of all texture weights = 1
					splatWeights[i] /= z;
					
					// Assign this point to the splatmap array
					splatmapData[x, y, i] = splatWeights[i];
				}
			}
		}
		
		// Finally assign the new splatmap to the terrainData:
		terrainData.SetAlphamaps(0, 0, splatmapData);
	}

}