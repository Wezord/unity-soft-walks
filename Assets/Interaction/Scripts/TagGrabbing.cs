using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TagGrabbing : MonoBehaviour
{
	public PlayerType type;
	public System.Action collided;

	private void OnCollisionEnter(Collision collision)
	{
		var o = collision.gameObject;
		if (Utils.IsLayerType(collision.gameObject.layer, type))
		{
			collided();
			Destroy(this);
		}
	}
}
