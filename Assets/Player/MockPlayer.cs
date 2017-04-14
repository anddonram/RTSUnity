using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
public class MockPlayer : Player {
	protected override void Start ()
	{
	}
	protected override void Update ()
	{
	}
	public override void OnStartLocalPlayer ()
	{
		localPlayer = this;
	}
	public override bool IsDead ()
	{
		return true;
	}
}
