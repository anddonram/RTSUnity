using UnityEngine;
using System.Collections;

public class Node1 : Node {
	public TechTree techTree;
	protected override void ApplyEffect ()
	{
		Player p = techTree.player;
		p.IncreaseResourceLimit (RTS.ResourceType.Population,5);
	}
}
