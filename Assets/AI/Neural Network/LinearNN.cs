using UnityEngine;
using System.Collections;

public class LinearNN : NeuralNetwork {
	public LinearNN (int numInput,int numHidden,int numOutput,float cte=0.5f,bool random=false):base( numInput, numHidden, numOutput,cte,random){

	}
	public override float Function (float input)
	{
		return input;
	}
	public override float FunctionDerivative (float input)
	{
		return 1;

	}
}
