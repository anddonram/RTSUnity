using UnityEngine;
using System.Collections;
using RTS;
using UnityEngine.Networking;
public class TechTree : MonoBehaviour
{
	private Node[] nodes;
	public Player player;
	void Start(){
		nodes = GetComponentsInChildren<Node> (true);
	
	}
	public bool IsDevelopable (Node node)
	{
		bool res = node.IsAvailable ();
		if (res) {
			foreach (ResourceType r in ResourceManager.ResourceTypes) {
				if (node.GetCost (r) > player.GetResource (r)) {
					res = false;
					break;
				}
			}
		}
		return res;
	}
	public Node[] GetNodes(){
		return nodes;
	}
	public Node GetNode(string nodeName)
	{
		Node node = null;
		foreach (Node n in nodes)
			if (n.nodeName.Equals (nodeName)) {
				node = n;
				break;
			}
		return node;
	}
}
