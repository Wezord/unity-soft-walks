using System;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

//[RequireComponent(typeof(Rigidbody), typeof(ConfigurableJoint))]
public class BotJoint : MonoBehaviour
{

	public enum SIDE
	{
		LEFT = 0,
		CENTER = 1,
		RIGHT = 2
	}
	
    public float bodyPercentage = 0f;

	public float baseRadius = 1f;

	public SIDE side;
    
    public bool configuring = false;

    public bool mirroring = false;

	public float collisionPenalty = 1f;

    [Range(-1f, 1f)]
    public float actionX;
	[Range(-1f, 1f)]
	public float actionY;
	[Range(-1f, 1f)]
	public float actionZ;

	public float torqueFactor = 1f;
    public float springFactor = 1f;
    public float damperFactor = 1f;

	[NonSerialized]
	public Rigidbody body;
	[NonSerialized]
	public ConfigurableJoint joint;
	[NonSerialized]
	public BotParameters p;
	[NonSerialized]
	public BotJoint parent = null;
	[NonSerialized]
	public List<BotJoint> children;

	public bool isFrozen = false;

	private bool initialized = false;

	private int collisionCount = 0;

	private RigidbodyCopy copy;

    private bool xLocked = false;
    private bool yLocked = false;
    private bool zLocked = false;

	[NonSerialized]
	public float colliderVolume = 0;

    [NonSerialized]
    public Quaternion startLocalRotation;

    private Vector3 lastPosition;
	[NonSerialized]
	public Vector3 realVelocity;

	// test pd controller
	private float kp;
	private float kd;

	[NonSerialized]
	public Referential referential;

	[NonSerialized]
	public Vector3 normalObs, tangentObs, velocityObs, angularVelocityObs;

	private BotJointLimitsConfigurator limitsConfigurator;

	[NonSerialized]
	public bool passNormalObs = true, passTangentObs = true, passVelocityObs = true, passAngularVelocityObs = false;

	private float angularVelocityFactor = 0f;

	private string partName;

	private void Start()
	{
		Initialize();
	}

	public void Initialize()
    {
        if (initialized) return;

		body = GetComponent<Rigidbody>();
		joint = GetComponent<ConfigurableJoint>();

		if (IsBroken()) return;

		copy = new RigidbodyCopy(transform, body);

		body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

		xLocked = joint.angularXMotion != ConfigurableJointMotion.Locked;
		yLocked = joint.angularYMotion != ConfigurableJointMotion.Locked;
		zLocked = joint.angularZMotion != ConfigurableJointMotion.Locked;
		
		limitsConfigurator = GetComponent<BotJointLimitsConfigurator>();
		
		/*
        joint.xMotion = ConfigurableJointMotion.Locked;
		joint.yMotion = ConfigurableJointMotion.Locked;
		joint.zMotion = ConfigurableJointMotion.Locked;
		*/

        joint.enablePreprocessing = false;

		parent = joint.connectedBody ? joint.connectedBody.GetComponent<BotJoint>() : null;
		if (parent != null) parent.AddChild(this);

        startLocalRotation = transform.localRotation;

		var strs = gameObject.name.Split(':');
		partName = strs[strs.Length - 1];

		initialized = true;
	}

	public void Initialize(BotParameters p)
    {
        Initialize();

        this.p = p;

		referential = parent ? new UnorthogonalReferential(parent.transform) : p.agent.referential;

		//if(!p.recording) body.collisionDetectionMode = CollisionDetectionMode.Discrete;

        Configure();
		Restart();

	}

    private void Configure()
    {
        Initialize();

        body.mass = p.totalMass * bodyPercentage/100f;

		body.maxAngularVelocity = p.maxAngularVelocity;

		joint.rotationDriveMode = RotationDriveMode.Slerp;
		var drive = joint.slerpDrive;
		drive.maximumForce = p.maxTorque * p.torqueFactor * torqueFactor;
		joint.slerpDrive = drive;

		joint.breakForce = p.breakForce;
        joint.breakTorque = p.breakTorque;
    }

    private void FixedUpdate()
    {
        
        if (IsBroken()) 
			return;

		//if(Application.isEditor) Configure();
		CalculateVelocity();
		var targetRotation = GetTargetRotation();
        joint.targetRotation = targetRotation;
        
    }

	public void Restart()
    {
        Initialize();
		if(copy != null) copy.paste(transform, body);
        if (!body.isKinematic)
        {
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }
		lastPosition = transform.position;
	}


    public void CalculateObservations(Referential rootReferential)
	{

		if (IsBroken() || isFrozen) return;

		var primary = joint.axis;
		var secondary = joint.secondaryAxis;

        if(side == SIDE.RIGHT)
		{
            primary = -primary;
            secondary = -secondary;
        }

		UpdateReferential();
		var referential = GetReferential(rootReferential);

        primary = transform.TransformDirection(primary);
        secondary = transform.TransformDirection(secondary);
		normalObs = referential.InverseTransformVector(primary);
		tangentObs = referential.InverseTransformVector(secondary);
		if (p.normalizedVelocity)
		{
			var diff = joint.connectedBody.transform.TransformPoint(joint.connectedBody.centerOfMass) - transform.TransformPoint(body.centerOfMass);
			velocityObs = referential.InverseTransformNormalizedVelocity(realVelocity, diff, baseRadius);
		}
		else
		{
			velocityObs = referential.InverseTransformVelocity(realVelocity);
		}
		angularVelocityObs = referential.InverseTransformAngularVelocity(body.angularVelocity);

	}

    public void CollectObservations(VectorSensor sensor)
    {
        if(passNormalObs) sensor.AddObservation(normalObs);
		if(passTangentObs) sensor.AddObservation(tangentObs);
		if(passVelocityObs) sensor.AddObservation(velocityObs);
		if(passAngularVelocityObs) sensor.AddObservation(angularVelocityObs);
	}

	public void CollectAllObservations(VectorSensor sensor)
	{
		sensor.AddObservation(normalObs);
		sensor.AddObservation(tangentObs);
		sensor.AddObservation(velocityObs);
		sensor.AddObservation(angularVelocityObs);
	}

	public Referential GetReferential(Referential rootReferential)
    {
        return p.jointSpace ? referential : rootReferential;
    }

	private void UpdateReferential()
	{
		if (parent != null) referential.rootVelocity = p.relativeVelocity ? parent.realVelocity : Vector3.zero;
	}

    public float SpaceMagnitude(ArticulationReducedSpace space)
    {
        float acc = 0;
        for(int i = 0; i<space.dofCount; i++)
        {
            acc += space[i] * space[i];
        }
        return Mathf.Sqrt(acc);
    }

    public void OnActionReceived(ActionBuffers actions, ref int i)
    {

        if(IsBroken())
        {
			if (p.springControl) i += 2;
            if (xLocked) i++;
            if (yLocked) i++;
            if (zLocked) i++;
            return;
        }

		if(p.springControl)
		{
			float spring = (actions.ContinuousActions[i++] + 1f) / 2f;
			float damper = (actions.ContinuousActions[i++] + 1f) / 2f;

			SetDrive(spring, damper);

			p.agent.RecordStat("Bot Joint/Spring Factor", spring, StatAggregationMethod.Histogram);
			p.agent.RecordStat("Bot Joint/Damper Factor", damper, StatAggregationMethod.Histogram);
			p.agent.RecordStat("Bot Joint/Spring", joint.slerpDrive.positionSpring, StatAggregationMethod.Histogram);
			p.agent.RecordStat("Bot Joint/Damper", joint.slerpDrive.positionDamper, StatAggregationMethod.Histogram);
			p.agent.RecordStat("Bot Joint/Angular Velocity Factor", angularVelocityFactor, StatAggregationMethod.Histogram);

			p.agent.RecordStat("Bot Joint/Spring Factor " + partName, spring, StatAggregationMethod.Histogram);
			p.agent.RecordStat("Bot Joint/Damper Factor " + partName, damper, StatAggregationMethod.Histogram);
			p.agent.RecordStat("Bot Joint/Spring " + partName, joint.slerpDrive.positionSpring, StatAggregationMethod.Histogram);
			p.agent.RecordStat("Bot Joint/Damper " + partName, joint.slerpDrive.positionDamper, StatAggregationMethod.Histogram);
			p.agent.RecordStat("Bot Joint/Angular Velocity Factor " + partName, angularVelocityFactor, StatAggregationMethod.Histogram);
		} else
		{
			SetDrive(0.5f, 0.5f);
		}

		Vector3 action = Vector3.zero;
        if (joint.angularXMotion != ConfigurableJointMotion.Locked)
			action.x = actions.ContinuousActions[i++];
        if (joint.angularYMotion != ConfigurableJointMotion.Locked)
			action.y = actions.ContinuousActions[i++];
        if (joint.angularZMotion != ConfigurableJointMotion.Locked)
			action.z = actions.ContinuousActions[i++];

        if(!configuring && !p.immobile)
        {
            actionX = action.x; actionY = action.y; actionZ = action.z;
		}

		if(p.immobile)
		{
			SetDrive(100, 100);
		}

		var targetRotation = GetTargetRotation();

        if(!p.immobile) joint.targetRotation = targetRotation;
        Vector3 error = joint.GetTargetAngularVelocityToTargetRotation(startLocalRotation);
        joint.targetAngularVelocity = error * angularVelocityFactor;
    }

    public Quaternion GetTargetRotation()
    {
        Vector3 targetRotation = Vector3.zero;

		if (joint.angularXMotion != ConfigurableJointMotion.Locked)
		{
			targetRotation.x = actionX;
			targetRotation.x *= (joint.highAngularXLimit.limit - joint.lowAngularXLimit.limit) / 2;
			targetRotation.x += (joint.highAngularXLimit.limit + joint.lowAngularXLimit.limit) / 2;
		}
		if (joint.angularYMotion != ConfigurableJointMotion.Locked)
			targetRotation.y = actionY * joint.angularYLimit.limit;
		if (joint.angularZMotion != ConfigurableJointMotion.Locked)
			targetRotation.z = actionZ * joint.angularZLimit.limit;

		/*
		if (mirroring && side == SIDE.CENTER)
		{
			targetRotation.z = -targetRotation.z;
			targetRotation.y = -targetRotation.y;
		}
		*/

		return Quaternion.Euler(targetRotation);
	}

	public Vector3 GetTargetRotationActions(Quaternion targetRotation)
	{
		Vector3 rotations = targetRotation.eulerAngles;
		rotations = new Vector3(SmallestAngle(rotations.x), SmallestAngle(rotations.y), SmallestAngle(rotations.z));

		Vector3 actions = Vector3.zero;

		if (joint.angularXMotion != ConfigurableJointMotion.Locked)
		{
			actions.x = rotations.x;
			actions.x -= (joint.highAngularXLimit.limit + joint.lowAngularXLimit.limit) / 2;
			actions.x = actions.x % 360;
			actions.x /= (joint.highAngularXLimit.limit - joint.lowAngularXLimit.limit) / 2;
		} else if (rotations.x != 0)
		{
			Debug.Log(name + " locked x rotation : " + rotations.x);
		}
		if (joint.angularYMotion != ConfigurableJointMotion.Locked)
		{
			actions.y = rotations.y / joint.angularYLimit.limit;
		} else if(rotations.y != 0)
		{
			Debug.Log(name + " locked y rotation : " + rotations.y);
		}
		if (joint.angularZMotion != ConfigurableJointMotion.Locked)
		{
			actions.z = rotations.z / joint.angularZLimit.limit;
		}
		else if (rotations.z != 0)
		{
			Debug.Log(name + " locked z rotation : " + rotations.z);
		}

		if(limitsConfigurator){
			limitsConfigurator.UpdateLimits(rotations, actions);
		}

		return actions;

	}

	public float SmallestAngle(float angle)
	{
		var smallerAngle = angle - 360;
		return Mathf.Abs(angle) > Mathf.Abs(smallerAngle) ? smallerAngle : angle;
	}

    public void SetDrive(float springCoefficient = 1f, float damperCoefficient = 1f)
    {
		var drive = joint.slerpDrive;
		drive.positionSpring = p.rotationSpring * springCoefficient * p.torqueFactor * torqueFactor * springFactor;
        angularVelocityFactor = p.angularVelocityFactor * springCoefficient;
		drive.positionDamper = p.rotationDamper * damperCoefficient * p.torqueFactor * torqueFactor * damperFactor;
		drive.maximumForce = p.maxTorque * p.torqueFactor * torqueFactor;
		joint.slerpDrive = drive;

		// test pd controller
		kp = springCoefficient;
		kd = damperCoefficient;
	}

	public Vector3 GetLocalRotationActions(Quaternion localRotation)
    {
		Quaternion targetRotation = joint.GetTargetRotationLocal(localRotation, startLocalRotation);
		return GetTargetRotationActions(targetRotation);
	}

	public float GetConsumedEnergy()
	{
	    // fix from main
		if(!joint) return 0;
		// Get error as angle difference between target and current rotation
		var error = Quaternion.Angle(joint.targetRotation, joint.GetTargetRotationLocal(transform.localRotation, startLocalRotation));
		var derror = body.angularVelocity.magnitude;
		// Apply spring formula from PhysX docs
		var torque = joint.slerpDrive.positionSpring * error + joint.slerpDrive.positionDamper * derror;
		torque = Mathf.Min(joint.slerpDrive.maximumForce, torque);
		return torque;

		// // test pd controller
		// if(!joint) return 0;
		// float dt = Time.fixedDeltaTime;
		// float g = 1 / (1 + kd * dt + kp * dt * dt);
		// float ksg = kp * g;
		// float kdg = (kd + kp * dt) * g;
		
		// Quaternion q = GetTargetRotation() * Quaternion.Inverse(transform.rotation);
		// // Q can be the-long-rotation-around-the-sphere eg. 350 degrees
		// // We want the equivalant short rotation eg. -10 degrees
		// // Check if rotation is greater than 190 degees == q.w is negative
		// if (q.w < 0){
		// 	// Convert the quaterion to eqivalent "short way around" quaterion
		// 	q.x = -q.x;
		// 	q.y = -q.y;
		// 	q.z = -q.z;
		// 	q.w = -q.w;
		// }
		// Vector3 x;
		// float xMag;
		// q.ToAngleAxis(out xMag, out x);
		// x.Normalize();
		// x *= Mathf.Deg2Rad;
		// var rigidbody =  GetComponent<Rigidbody>();
		// Vector3 pidv = ksg * x * xMag - kdg * rigidbody.angularVelocity;
		// Quaternion rotInertia2World = rigidbody.inertiaTensorRotation * transform.rotation;
		// pidv = Quaternion.Inverse(rotInertia2World) * pidv;
		// pidv.Scale(rigidbody.inertiaTensor);
		// pidv = rotInertia2World * pidv;
		// return pidv.magnitude;
		// //return joint ? joint.currentTorque.magnitude : 0;
	}

    private void CalculateVelocity()
    {
		realVelocity = (transform.position - lastPosition) / Time.fixedDeltaTime;
		lastPosition = transform.position;
	}

    public bool IsBroken()
    {
        return joint == null;
    }

    public void AddChild(BotJoint child)
    {
        if(children == null) children = new List<BotJoint>();
        children.Add(child);
    }

	public void SetFreezing(bool freezing)
	{
		if(freezing) body.constraints = RigidbodyConstraints.FreezeAll;
	}

	public void ApplyDamage(Collision collision)
    {
		var collisionForce = collision.relativeVelocity.magnitude * body.mass;
		var reward = -p.collisionPenalty * collisionPenalty * collisionForce;

		p.agent.stepCollisionPenalty += reward;
		
		p.agent.RecordStat("Collision/Collision Force", collisionForce, StatAggregationMethod.Histogram);
		p.agent.RecordStat("Collision/Collision Penalty", reward, StatAggregationMethod.Histogram);

		p.agent.AddReward(reward);

        if (collision.rigidbody && collision.rigidbody.TryGetComponent<DamageStrength>(out var damage))
        {
            var deduction = 4f * damage.strength * collision.relativeVelocity.sqrMagnitude * body.mass;
            DeductBreakForce(joint, deduction);
            if (children != null)
            {
                foreach (var child in children)
                {
                    DeductBreakForce(child.joint, deduction);
                }
            }
        }
    }

    public void DeductBreakForce(ConfigurableJoint joint, float deduction)
    {
        if (joint == null) return;
        joint.breakForce -= deduction;
        joint.breakTorque -= deduction;

        if (joint.breakForce < 0 || joint.breakTorque < 0)
        {
            Destroy(joint);
			/*
            var e = GetComponent<JointBreakEvent>();
            if(e != null)
            {
                e.OnJointBreak(deduction);
            }
			*/
        }
    }

	void OnCollisionEnter(Collision collision)
	{
		collisionCount++;
	}

	private void OnCollisionStay(Collision collision)
	{
		ApplyDamage(collision);
	}

	void OnCollisionExit()
    {
        collisionCount--;
    }

	void OnTriggerEnter()
    {
		// if making demonstrations, set to all contact pairs in settings since will be kinematic
		collisionCount++;
	}

	void OnTriggerExit()
	{
		collisionCount--;
	}

	void OnJointBreak(float breakForce)
	{
		var layer = gameObject.layer = LayerMask.NameToLayer("Default");
		foreach (Transform child in GetComponentsInChildren<Transform>())
		{
			child.gameObject.layer = layer;
		}
	}

	private void OnEnable()
	{
		startLocalRotation = transform.localRotation;
		if (joint)
		{
			joint.axis = joint.axis;
			joint.secondaryAxis = joint.secondaryAxis;
		}
	}

}
