using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TagGrabbed : MonoBehaviour
{
    public PlayerType type;
    public System.Action collided;
	public LayerMask mask;

	private void OnCollisionStay(Collision collision)
	{
		var o = collision.gameObject;
		if (Utils.IsLayerType(collision.gameObject.layer, type))
		{
			collided();
			Debug.Log("touched other player");
			Destroy(this);
		} else if (((1 << o.layer) & mask) == 0)
		{
			Debug.Log("touched something else");
			Destroy(this);
		}
	}
}
