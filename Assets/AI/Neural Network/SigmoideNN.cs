using UnityEngine;
using System.Collections;

public class SigmoideNN : NeuralNetwork {
	public SigmoideNN (int numInput,int numHidden,int numOutput,float cte=0.5f,bool random=false):base( numInput, numHidden, numOutput,cte,random){

	}
	public override float Function (float input)
	{
		return 1 / (1 + Mathf.Exp (-input));
	}
	public override float FunctionDerivative (float input)
	{
		float res = Function (input);
		return res * (1 - res);
	}
}
