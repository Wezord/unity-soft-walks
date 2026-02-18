using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{

    public Transform axis;

    public float angularVelocityFactor = 0.1f;

    private Rigidbody body;
    private ConfigurableJoint joint;
    private Quaternion startLocalRotation, baseRotation;

    void Start()
    {
        body = GetComponent<Rigidbody>();
        joint = GetComponent<ConfigurableJoint>();

		baseRotation = transform.localRotation;
		startLocalRotation = transform.localRotation;
    }

    void FixedUpdate()
    {

        Quaternion target = Quaternion.Inverse(transform.parent.rotation) * axis.rotation * baseRotation;
        //target.Normalize();


        joint.SetTargetRotationLocal(target, startLocalRotation);

        Vector3 error = joint.GetTargetAngularVelocity(target);
        //Vector3 error = target.eulerAngles - transform.localRotation.eulerAngles;
        error.y = 0;
        //joint.targetAngularVelocity = error * angularVelocityFactor;
        //body.angularVelocity = -error * angularVelocityFactor;
        
    }

    /*
    public Quaternion getRotation()
    {
        var rotation = transform.localRotation * Quaternion.Inverse(startLocalRotation) * transform.parent.rotation;
        rotation = Quaternion.LookRotation(rotation * Vector3.forward, Vector3.up);
        return rotation;
    }
    */

	private void OnEnable()
	{
		if (joint)
		{
			joint.axis = joint.axis;
			joint.secondaryAxis = joint.secondaryAxis;
			startLocalRotation = transform.localRotation;
		}
	}

}
