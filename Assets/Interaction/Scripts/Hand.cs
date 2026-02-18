using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using System;

public class Hand : MonoBehaviour
{

    public bool rightHanded;

    [HideInInspector]
    public VRPlayer player;

    private ConfigurableJoint joint;

    private GameObject selecting = null;

    [HideInInspector]
    public Rigidbody body;

    [HideInInspector]
    public Grabber grab;

    public float maxForce = 100f;
    public float maxTorque = 100f;
    public float forceFactor = 1f;
    public float vibrationThreshold = 1f;
    public float vibrationFactor = 1f;
    public float vibrationLength = 1;


    public Finger[] fingers = new Finger[5];

    public Device device;

    public Vector3 position;
    public Vector3 velocity;
    public Quaternion rotation;
    public Vector3 angularVelocity;
    
    public Vector3 target;

    private PIDVector3 linearPID;
    private PIDVector3 angularPID;

    [HideInInspector]
    public Parameters linearParameters;
	[HideInInspector]
	public Parameters angularParameters;

    private RigidbodyCopy startBody;
    private Quaternion startLocalRotation;

    [NonSerialized]
	public bool gripping = false;

	public void Initialize(VRPlayer player)
    {

        this.player = player;
        foreach (Finger finger in fingers)
        {
            finger.Initialize(this);
        }
    }

    private void Start()
    {
        body = GetComponent<Rigidbody>();
        startBody = new RigidbodyCopy(transform, body);

        joint = GetComponent<ConfigurableJoint>();
        grab = GetComponent<Grabber>();

        linearParameters = GetComponents<Parameters>()[0];
        angularParameters = GetComponents<Parameters>()[1];

        linearPID = new PIDVector3(linearParameters);
        angularPID = new PIDVector3(angularParameters);

        device = player.VR ? new Device(rightHanded ? XRNode.RightHand : XRNode.LeftHand) : new Device();

        startLocalRotation = transform.localRotation;
    }

    void FixedUpdate()
    {
        Vector3 error = GetLinearError();
        
        if(error.magnitude > 1)
        {
            setToTarget();
        }
        Input();

        //Move();
        JointMove();

        MoveFingers();
	}

    public void Vibrate(Collision collision)
    {
        
        if (collisions > 1) return;


        var mag = collision.relativeVelocity.magnitude;
        if (mag > vibrationThreshold)
        {
            var amplitude = Mathf.Clamp01(mag * vibrationFactor);
            device.Vibrate(amplitude, vibrationLength);
        }
        
    }

    public void setToTarget()
    {
        transform.position = player.body.position - (player.position - position);
        transform.rotation = rotation;
        body.velocity = velocity;
        body.angularVelocity = angularVelocity;
    }

    private Vector3 lastPosition = new Vector3(0, 0, 0);
    private Vector3 lastVelocity = new Vector3(0, 0, 0);
    private Vector3 lastForce = new Vector3(0, 0, 0);
    private Vector3 lastLError = new Vector3(0, 0, 0);
    private Vector3 lastLdError = new Vector3(0, 0, 0);
    private float lload = 1.0f;

    private Vector3 lastTorque = new Vector3(0, 0, 0);
    private Vector3 lastAError = new Vector3(0, 0, 0);
    private Vector3 lastAdError = new Vector3(0, 0, 0);
    private float aload = 1.0f;


    private void Input()
    {

        device.query(CommonUsages.devicePosition, out this.position);
        device.query(CommonUsages.deviceVelocity, out this.velocity);

        device.query(CommonUsages.deviceRotation, out this.rotation);
        device.query(CommonUsages.deviceAngularVelocity, out this.angularVelocity);

    }

    private void JointMove()
    {

        //float grabbedMass = grab.grabbed && grab.grabJoint.connectedBody ? grab.grabJoint.connectedBody.mass : player.body.mass;
        //grabbedMass = Mathf.Max(grabbedMass, player.body.mass);
        float mass = body.mass;

        { // Linear Drive

            JointDrive drive = new JointDrive();
            drive.positionSpring = Mathf.Exp(linearParameters.p1 / 100) * mass * forceFactor; //1700
            drive.positionDamper = Mathf.Exp(linearParameters.p2 / 100) * mass * forceFactor; //1256.839
            drive.maximumForce = Mathf.Exp(linearParameters.p3 / 100); //935.1085
            drive.maximumForce = maxForce;

            this.joint.xDrive = drive;
            this.joint.yDrive = drive;
            this.joint.zDrive = drive;

            
            Vector3 target = player.body.position + position - player.position;
            Vector3 error = GetLinearError();
            

            Vector3 derror = (error - lastLError) / Time.deltaTime;

            joint.connectedAnchor = position - player.position;

			Vector3 finalVelocity = player.transform.InverseTransformVector(velocity) * Mathf.Max(1f - error.magnitude, 0f);
            Vector3 targetVelocity = player.transform.InverseTransformVector(error) / Time.fixedDeltaTime * Mathf.Exp(linearParameters.p4 / 100);

            joint.targetVelocity = finalVelocity + targetVelocity;

		}

        { // Angular Drive
            Vector3 error = GetAngularError();

            JointDrive drive = this.joint.slerpDrive;

            drive.positionSpring = Mathf.Exp(angularParameters.p1 / 100) * mass * forceFactor; //1212.402
            drive.positionDamper = Mathf.Exp(angularParameters.p2 / 100) * mass * forceFactor; //502.0697
            drive.maximumForce = Mathf.Exp(angularParameters.p3 / 100); //703.3912
            drive.maximumForce = maxTorque;
            joint.slerpDrive = drive;

            joint.targetRotation = startLocalRotation * rotation;
            //ConfigurableJointExtensions.SetTargetRotationLocal(joint, rotation, startLocalRotation);// player.transform.rotation * rotation;
            //joint.targetAngularVelocity = angularVelocity + error * Mathf.Exp(angularParameters.p4 / 100);
            joint.swapBodies = true;

			Vector3 finalVelocity = angularVelocity * Mathf.Max(1f - error.magnitude, 0f);
            Vector3 targetVelocity = error / Time.fixedDeltaTime * Mathf.Exp(angularParameters.p4 / 100);

			joint.targetAngularVelocity = finalVelocity + targetVelocity;

		}

    }

    private void Move()
    {

        {
            // Linear

            

            /*
             * e = (ptp - htp) - (pp - hp)
             */

            Vector3 error = GetLinearError();

            Vector3 derror = (error - lastLError) / Time.deltaTime;

            Vector3 dderror = (derror - lastLdError) / Time.deltaTime;

            Vector3 lastPrediction = (lastForce * (1.0f / body.mass + 1.0f / player.body.mass));


            //Vector3 vload = Utils.vclamp(Utils.vdiv(lastPrediction, dderror), 0.0001);

            float sload = Math.Max(Math.Max(lastForce.magnitude, 0.000001f) / Math.Max(dderror.magnitude, 0.000001f), 1);

            //lload = Utils.smov(lload, sload, linearParameters.p3);
            lload = Math.Clamp(lload * linearParameters.p3, 1, sload);


            /*
            if (Vector3.Distance(target, transform.position) > 2)
            {
                transform.position = target;
                body.velocity = velocity;
            }
            */


            //Vector3 force = linearPID.get(error);


            /*
             * v2 = v1 + F * dt / m
             * => F = m * (v2 - v1) / dt
             * 
             * p2 = p1 + v2 * dt
             * => v2 = (p2 - p1) / dt
             * 
             * => F = m * ((p2 - p1) / dt - v1) / dt
             * 
             */


            // 0.5
            Vector3 force = (error * linearParameters.p1 / Time.deltaTime /*- body.velocity*/) / Time.deltaTime;

            //force = Utils.vmul(force, vload);
            force *= lload;

            //force += F / linearParameters.p3;


            force = Vector3.ClampMagnitude(force, maxForce);

            //force += body.velocity / Mathf.Max(error.magnitude, 0.0001f) * linearParameters.p2;

            // 60
            force += derror * linearParameters.p2 * lload;

            //body.drag = linearParameters.p2 * 0.1f;
            //player.body.drag = linearParameters.p2 * 0.1f;

            force = Vector3.ClampMagnitude(force, 3*maxForce);

            if (Utils.IsFinite(force))
            {
                body.AddForce(force);
                player.body.AddForce(-force);
            }
            else
            {
                force = new Vector3();
            }

            lastForce = force;
            lastLError = error;
            lastLdError = derror;
            lastPosition = transform.position;
            lastVelocity = body.velocity;

        }


        {
            // Angular

            Vector3 error = GetAngularError();

            Vector3 derror = body.angularVelocity;

            Vector3 dderror = (derror - lastAdError) / Time.deltaTime;

            float sload = Math.Max(Math.Max(lastTorque.magnitude, 0.000001f) / Math.Max(dderror.magnitude, 0.000001f), 1);

            aload = Math.Clamp(aload * linearParameters.p3, 1, sload);

            //aload = sload;

            //0.8536277
            Vector3 torque = (error / Time.deltaTime - derror) / Time.deltaTime * angularParameters.p1;
            torque *= aload;

            //force = mul(force, load);

            //torque *= 0;

            //torque = Vector3.ClampMagnitude(torque, maxTorque);

            //19.56606
            torque += derror * angularParameters.p2 * aload;


            //body.angularDrag = angularParameters.p2;

            //body.angularVelocity *= (1.0f-angularParameters.p2);

            /*
            float frequency = angularParameters.p1;
            float damping = angularParameters.p2;

            float kp = (6f * frequency) * (6f * frequency) * 0.25f;
            float kd = 4.5f * frequency * damping;
            float dt = Time.fixedDeltaTime;
            float g = 1 / (1 + kd * dt + kp * dt * dt);
            float ksg = kp * g;
            float kdg = (kd + kp * dt) * g;

            Vector3 torque = kp * error - kd * body.angularVelocity;
            */

            if (rightHanded)
            {
                //Debug.Log(sload);
                //Debug.Log(Time.fixedTime + " " + error.x + " " + error.y + " " + error.z + " " + torque.x + " " + torque.y + " " + torque.z + " " + load.x + " " + load.y + " " + load.z);
            }
            
            Quaternion rotInertia2World = body.inertiaTensorRotation * transform.rotation;
            torque = Quaternion.Inverse(rotInertia2World) * torque;
            torque.Scale(body.inertiaTensor);
            torque = rotInertia2World * torque;
            
            if (Utils.IsFinite(torque))
            {
                body.AddTorque(torque);
            }
            else
            {
                torque = new Vector3();
            }

            lastTorque = torque;
            lastAError = error;
            lastAdError = derror;


        }

    }

    public Vector3 GetLinearError()
    {
        Vector3 trueError = player.transform.TransformPoint(position - player.position) - transform.position;
        return trueError;
    }

    public Vector3 GetAngularError()
    {
        Quaternion rot = rotation * Quaternion.Inverse(transform.localRotation);
        if (rot.w < 0)
        {
            rot.x = -rot.x;
            rot.y = -rot.y;
            rot.z = -rot.z;
            rot.w = -rot.w;
        }

        rot.ToAngleAxis(out float angle, out Vector3 axis);
        axis.Normalize();
        Vector3 error = angle * Mathf.Deg2Rad * axis;

        return error;
    }

    private void MoveFingers()
    {
        /*
        int layerMask = LayerMask.GetMask("Object");

        if (Physics.Raycast(transform.position, transform.right * (rightHanded ? -1f : 1f), out RaycastHit hitInfo, 100000f, layerMask))
        {

            Select(hitInfo.transform.gameObject);
            
        } else
        {
            Unselect();
        }
        */


        if (device.query(CommonUsages.grip, out float grip))
        {
            grab.SetGrabbing(grip > 0.5f);

            device.query(CommonUsages.trigger, out float triggerAmount);
            
            grab.SetRotationLock(triggerAmount > 0.50f);

            if(!grab.grabbed)
            {
				for (int i = 0; i < fingers.Length; i++)
				{
					fingers[i].position = grip;
				}
			}
        }
    }


    public void Select(GameObject obj)
    {

        if (obj != selecting)
        {

            Unselect();
            selecting = obj;

        }

        if (!selecting.GetComponent<Outline>())
        {

            Outline outline = selecting.AddComponent<Outline>();
            outline.OutlineWidth = 10;
            outline.OutlineColor = Color.black;

        }


    }

    public void Unselect()
    {
        if (selecting != null && selecting.GetComponent<Outline>())
        {
            Destroy(selecting.GetComponent<Outline>());
        }
    }

    private int collisions = 0;
    private void OnCollisionEnter(Collision collision)
    {
        ++collisions;
        Vibrate(collision);
    }

    private void OnCollisionExit()
    {
        --collisions;
        //device.StopVibration();
    }

	private void OnEnable()
	{
		if (joint)
		{
			joint.axis = joint.axis;
			joint.secondaryAxis = joint.secondaryAxis;
		}
	}

}