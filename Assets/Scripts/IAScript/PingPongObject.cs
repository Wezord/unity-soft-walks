using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PingPongObject : MonoBehaviour
{

    public Vector3 a;
    public Vector3 b;

    private Vector3 target;
    public float velocity = 1f;

    private Rigidbody body;

    private Vector3 startPosition;

    void Start()
    {
        body = GetComponent<Rigidbody>();
        target = a;
		startPosition = transform.position;
	}

    void FixedUpdate()
    {
        var worldTarget = target + startPosition;

        body.MovePosition(Vector3.MoveTowards(body.position, worldTarget, Time.fixedDeltaTime * velocity));
        if (Vector3.Distance(body.position, worldTarget) < Time.fixedDeltaTime * velocity)
        {
            if(a.Equals(target))
            {
                target = b;
            } else
            {
                target = a;
            }
        }
    }

    public void Toggle()
    {
        enabled = !enabled;
    }
}
