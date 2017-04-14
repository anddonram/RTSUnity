using UnityEngine;
using System.Collections;

public class BackPropagationAlgorithm : MonoBehaviour
{
	public int numInput = 2, numHidden = 2, numOutput = 1;
	public float[] inputs, outputs;
	public float learningFactor = 0.1f;
	public bool linear = false, random = false;
	public float cte = 0.5f;
	private NeuralNetwork nn;
	private float[] deltaOutput, deltaHidden;


	//	public float waitForSeconds=0.5f;
	//	WaitForSeconds wfs;
	//	void Awake(){
	//		wfs = new WaitForSeconds (waitForSeconds);
	//		Recreate ();
	//	}
	public int GetResults ()
	{
		nn.SetInputs (inputs);
		nn.CalculateOutputs ();
		float max = nn.outputResults [0];
		int index = 0;
		for (int i = 1; i < numOutput; i++) {
			if (nn.outputResults [i] >= max) {
				max = nn.outputResults [i];
				index = i;
			}
		}
		return index + 1;

	}

	public void Recreate ()
	{
		if (linear)
			nn = new LinearNN (numInput, numHidden, numOutput, cte: cte, random: random);
		else
			nn = new SigmoideNN (numInput, numHidden, numOutput, cte: cte, random: random);
		deltaHidden = new float[nn.numHidden];
		deltaOutput = new float[nn.numOutput];
	
		inputs = new float[nn.numInput];
		outputs = new float[nn.numOutput];
	}

	public void Train ()
	{
		if (inputs.Length == nn.numInput && outputs.Length == nn.numOutput) {
			nn.SetInputs (inputs);
			nn.CalculateOutputs ();
			for (int i = 0; i < nn.numOutput; i++) {
				deltaOutput [i] = nn.FunctionDerivative (nn.outputIntermediate [i]) * (outputs [i] - nn.outputResults [i]);
				nn.weightOutputActivation [i] -= learningFactor * deltaOutput [i];
			
			}
			for (int i = 0; i < nn.numHidden; i++) {
				deltaHidden [i] = nn.FunctionDerivative (nn.hiddenIntermediate [i]) * CalculateDeltaFunction (i);
				nn.weightHiddenActivation [i] -= learningFactor * deltaHidden [i];
				for (int j = 0; j < nn.numOutput; j++) {
					nn.weightOutput [i, j] += learningFactor * nn.hiddenResults [i] * deltaOutput [j];
				}

			}
			for (int i = 0; i < nn.numOutput; i++) {
				for (int j = 0; j < nn.numOutput; j++) {
					nn.weightHidden [i, j] += learningFactor * nn.inputs [i] * deltaHidden [j];
				}
			}
		}
	}

	float CalculateDeltaFunction (int node)
	{
		float res = 0;
		for (int i = 0; i < nn.numOutput; i++)
			res += deltaOutput [i] * nn.weightOutput [node, i];

		return res;
	}

	public void ResetInputs ()
	{
		for (int i = 0; i < inputs.Length; i++) {
			inputs [i] = 0;
		}
	}

	public void ResetOutputs ()
	{
		for (int i = 0; i < outputs.Length; i++) {
			outputs [i] = 0;
		}
	}

	public float xpos = 100, ypos = 40, width = 100, height = 30;

	void OnGUI ()
	{if (nn==null)
			return;
//		if (GUI.Button (new Rect (20, 40, 80, 20), "Train"))
//			Train ();
//		if (GUI.Button (new Rect (20, 80, 80, 20), "Recreate"))
//			Recreate ();
//		if (GUI.Button (new Rect (20, 120, 80, 20), "Reset")) {
//			ResetInputs ();
//			ResetOutputs ();
//		}
		float i = ShowGUI (inputs, ypos, "inputs: ");
		i = ShowGUI (outputs, i, "outputs: ");
		//	i = ShowGUI (nn.weightHidden,i,"weight hidden: ");
		//	i = ShowGUI (nn.weightOutput,i,"weight output: ");
		//	i = ShowGUI (deltaOutput,i,"delta output: ");
		if (nn.inputs != null)
			i = ShowGUI (nn.inputs, i, "nn inputs: ");	
		i = ShowGUI (nn.outputResults, i, "output results: ");
		//GUI.Box (new Rect (100, 80, 40*nn.weightOutputActivation.Length, 20),nn.weightOutputActivation);
	}

	float ShowGUI (float[] item, float pos, string message)
	{
		string s = string.Empty;
		foreach (float f in item) {
			s += f + ", ";
		}
		GUI.Box (new Rect (xpos, pos, width + 5 * s.Length, height), message + s);
		return ypos + pos;
	}

	float ShowGUI (float[,] item, float pos, string message)
	{
		string s = string.Empty;
		foreach (float f in item) {
			s += f + ", ";
		}
		GUI.Box (new Rect (xpos, pos, width + 5 * s.Length, height), message + s);
		return ypos + pos;
	}

}
