using UnityEngine;

[ExecuteAlways]
public class BotJointConfiguration : MonoBehaviour
{
    void Awake()
    {
		print("switching to articulationbody");
		SwitchToArticulationBodies();
	}

    void Update()
    {
	}

    void SwitchToArticulationBodies()
    {
		var body = GetComponent<Rigidbody>();
		if(body) SwitchToArticulationBody(body);

		var botjoints = GetComponentsInChildren<BotJoint>();

		foreach (BotJoint joint in botjoints)
		{
            var rigidbody = joint.GetComponent<Rigidbody>();
			var confjoint = joint.GetComponent<ConfigurableJoint>();
			
            if(rigidbody != null) SwitchToArticulationBody(rigidbody, confjoint);
		}
	}

	ArticulationBody SwitchToArticulationBody(Rigidbody rb, ConfigurableJoint joint = null)
    {

		var go = rb.gameObject;

		var linearDamp = rb.drag;
		var angularDamp = rb.angularDrag;
		var mass = rb.mass;
		var detection = rb.collisionDetectionMode;
		var dumbLimit = new SoftJointLimit();
		SoftJointLimit xHighLimit, xLowLimit, yLimit, zLimit;
		xHighLimit = xLowLimit = yLimit = zLimit = dumbLimit;
		JointDrive slerpDrive = new JointDrive();
		if(joint)
		{
			xHighLimit = joint.highAngularXLimit;
			xLowLimit = joint.lowAngularXLimit;
			yLimit = joint.angularYLimit;
			zLimit = joint.angularZLimit;

			slerpDrive = joint.slerpDrive;
		}


		if (joint) DestroyImmediate(joint);
		if(rb) DestroyImmediate(rb);
		
		var articulation = go.AddComponent<ArticulationBody>();

		articulation.linearDamping = linearDamp;
		articulation.angularDamping = angularDamp;
		articulation.mass = mass;
		articulation.collisionDetectionMode = detection;

			articulation.jointType = ArticulationJointType.SphericalJoint;
			articulation.swingYLock = ArticulationDofLock.LimitedMotion;
			articulation.swingZLock = ArticulationDofLock.LimitedMotion;
			articulation.twistLock = ArticulationDofLock.LimitedMotion;

			articulation.xDrive = MakeDrive(0, slerpDrive, xHighLimit, xLowLimit);
			articulation.yDrive = MakeDrive(1, slerpDrive, yLimit, yLimit);
			articulation.zDrive = MakeDrive(2, slerpDrive, zLimit, zLimit);

		return articulation;
	}

	ArticulationDrive MakeDrive(int axis, JointDrive drive, SoftJointLimit highLimit, SoftJointLimit lowLimit)
	{
		var ad = new ArticulationDrive();

		ad.upperLimit = highLimit.limit;
		ad.lowerLimit = lowLimit.limit;
		ad.forceLimit = drive.maximumForce;
		ad.stiffness = drive.positionSpring;
		ad.damping = drive.positionDamper;

		return ad;
	}


}
