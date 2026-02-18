using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hook : MonoBehaviour
{

    [HideInInspector]
    public Rigidbody body;

    [HideInInspector]
    public GrapplingHook grapplingHook;

    [HideInInspector]
    public LineRenderer lineRenderer;

    [HideInInspector]
    public FixedJoint joint = null;

    public bool hooked = false;

    void Start()
    {
        body = GetComponent<Rigidbody>();
        lineRenderer = GetComponent<LineRenderer>();
    }

    void FixedUpdate()
    {
        LayerMask mask = ~LayerMask.GetMask("Ungrabbable", "FPSPlayer", "VRPlayer");

        if (!hooked && Physics.Raycast(transform.position, body.velocity, out RaycastHit hit, body.velocity.magnitude * 1.5f * Time.fixedDeltaTime, mask))
        {
            Attach(hit);
        }

        lineRenderer.SetPosition(0, grapplingHook.aimAxis.position + grapplingHook.aimAxis.forward * 0.01f);
        lineRenderer.SetPosition(1, transform.position);

    }

    void Attach(RaycastHit hit)
    {
        hooked = true;
        transform.parent = hit.transform;
        transform.position = hit.point;
        body.constraints = RigidbodyConstraints.FreezePosition;
        joint = gameObject.AddComponent<FixedJoint>();
        if (hit.rigidbody) joint.connectedBody = hit.rigidbody;
        else if (hit.articulationBody) joint.connectedArticulationBody = hit.articulationBody;
        
        grapplingHook.Hook();
    }

}
