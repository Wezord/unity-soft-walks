using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resetter : MonoBehaviour
{

    private Vector3 position, velocity, angularVelocity;
    private Quaternion rotation;

    private Rigidbody body;

    void Start()
    {
        position = transform.position;
        rotation = transform.rotation;
        body = GetComponent<Rigidbody>();
        if(body != null)
        {
			velocity = body.velocity;
			angularVelocity = body.angularVelocity;
		}
    }

    public virtual void ResetTransform()
	{
		Events.SignalDestroy(gameObject);
		transform.position = position;
		transform.rotation = rotation;
		body = GetComponent<Rigidbody>();
		if (body != null)
		{
			body.velocity = velocity;
			body.angularVelocity = angularVelocity;
		}
	}

	public void ResetTransformRecursive()
	{
		ResetTransform();
		foreach(var res in GetComponentsInChildren<Resetter>())
		{
			res.ResetTransform();
		}
	}
}
