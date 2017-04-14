using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using RTS;

public class Rock : Other
{


	public Rigidbody rb;

	protected override void Awake ()
	{
		base.Awake ();
		owner = transform.root.GetComponent<Player> ();
		ownerId = owner.netId;
	}


	private Vector3 dir;
	private Vector3 start;
	private bool selected;
	[ClientCallback]
	void OnMouseOver ()
	{
		if (Input.GetMouseButtonDown (1)) {
			selected = true;
			start = Input.mousePosition;
		} else if (selected && Input.GetMouseButtonUp (1)) {
			dir =Input.mousePosition-start;
			if (dir != Vector3.zero) {
				dir.z = dir.y;
				dir.y = -1;
				selected = false;
			}
		}
	}
	[ClientCallback]
	protected override void OnMouseExit(){
		base.OnMouseExit ();
		selected = false;
	}
	public Vector3 GetDirection(){
		return dir;
	}
	[Server]
	public void Push (Unit unit)
	{
		if (rb.velocity.sqrMagnitude == 0f) {
			unit.getOwner ().RpcPush (gameObject);
		}
		unit.StopActions ();
	}
	[Server]
	public void Push(Vector3 receive){
		if (rb.velocity.sqrMagnitude == 0f&&receive!=Vector3.zero) {
			rb.isKinematic = false;
			rb.velocity = receive;
			rb.drag = 0.01f;
			rb.angularDrag = 0.01f;
		}
	}
	[ServerCallback]
	public void OnCollisionEnter (Collision other)
	{
		if (other.gameObject.CompareTag ("Ground"))
			return;
		
		WorldObject wo = other.transform.GetComponent<WorldObject> ();
		if (wo) {
			wo.TakeDamage ((int)Mathf.Min (rb.velocity.sqrMagnitude, 30));
			TakeDamage ((int)Mathf.Min (rb.velocity.sqrMagnitude, 15));
		}
	}

	[ServerCallback]
	public void OnCollisionStay (Collision other)
	{
		if (other.gameObject.CompareTag ("Ground"))
		if (rb.velocity.sqrMagnitude > 0) {
			rb.drag += rb.drag * Time.deltaTime;
			rb.angularDrag = rb.drag / 4;
		} else if(!rb.isKinematic)
			rb.isKinematic = true;
	}
	[ServerCallback]
	public void OnCollisionExit (Collision other){
		if (other.gameObject.CompareTag ("Ground")){
			rb.drag = 0.01f;
			rb.angularDrag = 0.01f;
	}
	}

	public override void OnStartClient ()
	{
	}
}
