using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TagCollider : MonoBehaviour
{
	public System.Action collided;
	public LayerMask mask;

	
	private void OnCollisionEnter(Collision collision)
	{
		var o = collision.gameObject;
		if((o.tag == "Weapon" && ((1<<o.layer)&mask) == 0) || o.TryGetComponent<TagGrabbed>(out var _) || o.TryGetComponent<TagGrabbed>(out var _))
		{
			collided();
		}
	}
}
