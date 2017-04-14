using UnityEngine;
using System.Collections;
using RTS;
public class Node2 : Node {
	public TechTree techTree;

	protected override void ApplyEffect ()
	{
		techTree.player.IncreaseResourceLimit(ResourceType.Money,200);
	}
}
