using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SpeedIndicator : MonoBehaviour
{
    public float length = 1f;
    public float maxLength = 10f;
    public Rigidbody rigidBody;
    private LineRenderer lineRenderer;
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        var diff = rigidBody.velocity * Time.fixedDeltaTime * length;
        if (diff.sqrMagnitude > maxLength*maxLength) {
            diff.Normalize();
            diff *= maxLength;
        }

		lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, transform.position - diff);
    }
}
