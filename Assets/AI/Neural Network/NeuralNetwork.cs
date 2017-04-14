using UnityEngine;
using System.Collections;
/**
 * neural network with 1 hidden layer
 */ 
public abstract class NeuralNetwork  {

	public int numInput, numHidden, numOutput;

	public float[] inputs;
	public float[,] weightHidden,weightOutput;
	public float[] weightHiddenActivation,weightOutputActivation;
	public float[] hiddenIntermediate, hiddenResults;
	public float[] outputIntermediate, outputResults;
//	public float[] hiddenDerivative, outputDerivative;
	public abstract float Function (float input);
	public abstract float FunctionDerivative (float input);

	public NeuralNetwork(int numInput,int numHidden,int numOutput,float cte=0.5f,bool random=false){
		this.numInput = numInput;
		this.numHidden = numHidden;
		this.numOutput = numOutput;

		this.weightHidden=new float[numInput,numHidden];
		this.weightOutput=new float[numHidden,numOutput];

		if (random) {
			for (int i = 0; i < numInput; i++) {
				for (int j = 0; j < numHidden; j++) {
					weightHidden [i, j] = Random.value;
				}
			}
			for (int i = 0; i < numHidden; i++) {
				for (int j = 0; j < numOutput; j++) {
					weightOutput [i, j] = Random.value;
				}
			}
		} else {
			for (int i = 0; i < numInput; i++) {
				for (int j = 0; j < numHidden; j++) {
					weightHidden [i, j] = cte;
				}
			}
			for (int i = 0; i < numHidden; i++) {
				for (int j = 0; j < numOutput; j++) {
					weightOutput [i, j] = cte;
				}
			}
		}

		this.weightHiddenActivation=new float[numHidden];
		this.weightOutputActivation=new float[numOutput];

		for (int j = 0; j < numHidden; j++) {
			weightHiddenActivation[j]=0.5f;
		}
		for (int j = 0; j < numOutput; j++) {
			weightOutputActivation[j]=0.5f;
		}

		this.hiddenIntermediate = new float[numHidden];
		this.hiddenResults = new float[numHidden];
		this.outputIntermediate = new float[numOutput];
		this.outputResults = new float[numOutput];
	}

	public void SetInputs(float[] inputs){
		if (inputs.Length == numInput) {
			this.inputs = inputs;
		}
	}
	public void CalculateOutputs(){
		if (inputs != null && inputs.Length == numInput) {
			for (int i = 0; i < numHidden; i++) {
				hiddenIntermediate [i] = CalculateIntermediate (i,false);
				hiddenResults[i]=Function (hiddenIntermediate[i]);
			}
			for (int i = 0; i < numOutput; i++) {
				outputIntermediate [i] = CalculateIntermediate (i,true);
				outputResults[i]=Function (outputIntermediate[i]);
			}
		}
	}
	private float CalculateIntermediate(int node,bool final){
		float res = 0;
		if (final) {
			for (int i = 0; i < numHidden; i++) {
				res += hiddenResults [i] * weightOutput [i,node];
			}
			res -= weightOutputActivation [node];
		} else {
			for (int i = 0; i < numInput; i++) {
				res += inputs [i] * weightHidden [i,node];
			}
			res -= weightHiddenActivation [node];

		}
		return res;
	}
}
