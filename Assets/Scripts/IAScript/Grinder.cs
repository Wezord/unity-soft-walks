using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grinder : MonoBehaviour
{
	public LayerMask noGrind;

	private List<GameObject> destroyList = new List<GameObject>();

	private void FixedUpdate()
	{
		foreach(GameObject go in destroyList)
		{
			if(go)
			{
				Events.SignalDestroy(go);
				Destroy(go);
			}
		}
		destroyList.Clear();
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (noGrind == (noGrind | (1 << collision.gameObject.layer)))
			return;

		if(collision.rigidbody != null || collision.articulationBody != null)
		{
			destroyList.Add(collision.gameObject);
		}
	}

}
