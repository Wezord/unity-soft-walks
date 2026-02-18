using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Speed : MonoBehaviour
{

    public double speed;
    public Vector3 speedVector;

    private Vector3 lastPosition;

	private void Awake()
	{
	    lastPosition = transform.position;
	}

	void FixedUpdate()
    {
		speedVector = (transform.position - lastPosition) / Time.fixedDeltaTime;
        speed = speedVector.magnitude;
        lastPosition = transform.position;
    }

    public void ResetPosition()
    {
		lastPosition = transform.position;
	}

}
