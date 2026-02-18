using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puller : MonoBehaviour
{

    public Vector3 force = Vector3.zero;

	public Transform center;
    public Transform target;

	private Rigidbody body;

    void Start()
    {
		body = GetComponent<Rigidbody>();
	}

    void FixedUpdate()
    {
        body.AddForce(force);
        target.position = center.position;
    }
}
