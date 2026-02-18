using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class Rope : MonoBehaviour
{
    public float length;
    public float segmentLength;
    [HideInInspector]
    public int count = 0;

    public GameObject ropeSegment;

    public bool created = false;


    private LineRenderer line;

    private void Start()
    {
        line = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        if (!created)
        {
            //Kill all the children
            while (transform.childCount > 0) DestroyImmediate(transform.GetChild(0).gameObject);
            created = true;
            CreateRope();
        }

        line.positionCount = count+1;
        for(int i = 0; i < transform.childCount; i++)
        {
            //The origin of the rope segment is shifted; @TODO profile this
            line.SetPosition(i, transform.GetChild(i).TransformPoint(0,-segmentLength/2,0));
        }
        line.SetPosition(line.positionCount - 1, transform.GetChild(transform.childCount - 1).TransformPoint(0, 0.5f * segmentLength, 0));

    }

	private void CreateRope()
    {
        count = 1;

        var currentRope = Instantiate(ropeSegment, transform);

        for (float y = segmentLength; y < length; y += segmentLength)
        {
            count++;

            //Instantiate and set position
            var newRope = Instantiate(ropeSegment, transform);
            newRope.transform.localPosition = Vector3.up * y;

            //Connect to previous
            var joint = currentRope.GetComponent<ConfigurableJoint>();
            var body = newRope.GetComponent<Rigidbody>();
            joint.connectedBody = body;

            //For next loop
            currentRope = newRope;
        }

        //Final rope isn't connected
        DestroyImmediate(currentRope.GetComponent<ConfigurableJoint>());
    }

}
