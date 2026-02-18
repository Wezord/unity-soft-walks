using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppliedForce : MonoBehaviour
{
    public float force, torque;

    private Rigidbody body;
    private ConfigurableJoint joint;

    void Start()
    {
        body = GetComponent<Rigidbody>();
        joint = GetComponent<ConfigurableJoint>();
    }

    void FixedUpdate()
    {
        force = joint.currentForce.magnitude;
        torque = joint.currentTorque.magnitude;
    }
}
