using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceMesurer : MonoBehaviour
{

    public float currentForce, currentTorque, maxForce, maxTorque;

	public ConfigurableJointMotion linearMotion = ConfigurableJointMotion.Locked, angularMotion = ConfigurableJointMotion.Locked;

	private ConfigurableJoint joint;

    void OnEnable()
    {
        joint = gameObject.AddComponent<ConfigurableJoint>();

		joint.xMotion = linearMotion;
		joint.yMotion = linearMotion;
		joint.zMotion = linearMotion;
		joint.angularXMotion = angularMotion;
		joint.angularYMotion = angularMotion;
		joint.angularZMotion = angularMotion;
		maxForce = maxTorque = currentForce = currentTorque = 0;
	}

    void FixedUpdate()
    {
        currentForce = joint.currentForce.magnitude;
		currentTorque = joint.currentTorque.magnitude;
		maxForce = Mathf.Max(maxForce, currentForce);
		maxTorque = Mathf.Max(maxTorque, currentTorque);
	}

	private void OnDisable()
	{
		Destroy(joint);
	}
}
