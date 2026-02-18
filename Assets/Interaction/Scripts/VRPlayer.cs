using UnityEngine;
using UnityEngine.XR;

[RequireComponent(typeof(Rigidbody))]
public class VRPlayer : GenericPlayer
{

    public static VRPlayer player = null;

    public static bool ARTICULATED = false;

    public Hand leftHand;
    public Hand rightHand;

    public Transform head;

    [HideInInspector]
    public Hand[] hands;

    public bool VR = true;
    public bool ghost = false;

    public bool VRActive = false;

    public Device device;

    [HideInInspector]
    public Vector3 position;
    [HideInInspector]
    public Vector3 velocity;
    [HideInInspector]
    public Quaternion rotation;
    [HideInInspector]
    public Vector3 angularVelocity;

    protected override void Initialize()
    {
        base.Initialize();

        player = this;

        hands = new Hand[2];
        hands[0] = leftHand;
        hands[1] = rightHand;

        foreach (Hand hand in hands)
        {
            hand.Initialize(this);
        }

        device = VR ? new Device(XRNode.CenterEye) : new Device();

        CheckEnabled();
    }

    protected void Update()
    {
		CheckEnabled();

		Move();
        
        // Temp disable turning
		Turn();

		OrientBody();
	}

    protected override void FixedUpdate()
    {

        CheckEnabled();

        base.FixedUpdate();

        ApplyForcesToHands();

    }

    void Move()
    {
        if (device.query(CommonUsages.devicePosition, out Vector3 position))
        {
            transform.position += transform.TransformVector(position - this.position);
            this.position = position;
        }

        device.query(CommonUsages.deviceVelocity, out this.velocity);

        device.query(CommonUsages.deviceRotation, out this.rotation);

        device.query(CommonUsages.deviceAngularVelocity, out this.angularVelocity);
        
    }

    private float turnRotation = 0;

    void Turn()
    {
        Vector2 axis = input.GetViewAxis();

        turnRotation = axis.x * 2;
        transform.rotation *= Quaternion.Euler(0, turnRotation, 0);
    }

    void OrientBody()
    {
        capsule.transform.rotation = Quaternion.Euler(0, camera.transform.rotation.eulerAngles.y, 0);
        head.rotation = camera.transform.rotation;
    }

    void ApplyForcesToHands()
    {
        foreach (Hand hand in hands)
        {
            hand.body.AddForce(appliedForce / body.mass * hand.body.mass);
        }
    }

    public override float GetHeight()
    {
        return playerHeight + position.y - 1.80f;
    }

    void CheckEnabled()
    {

        device.query(CommonUsages.userPresence, out bool present);

        VRActive = XRSettings.isDeviceActive && present;

        camera.SetActive(VR && VRActive);

    }

    public float headStickRadius = 0.5f;
    public float headStickSpeed = 1f;
    private Vector3 headStickOrigin = Vector3.zero;

    /*
    protected override void Walk()
    {

        device.query(CommonUsages.devicePosition, out Vector3 position);

		if (!input.GetWalkingButton())
        {
			headStickOrigin = position;
		}

		Vector3 axis = position - headStickOrigin;
        axis = transform.parent.TransformVector(axis);

        if(axis.sqrMagnitude < headStickRadius*headStickRadius)
        {
			axis = Vector3.zero;
		}

		Vector3 relativeSpeed = body.velocity - groundSpeed;

		if (axis.sqrMagnitude != 0)
		{
			float axeSpeed = Vector3.Dot(relativeSpeed, axis);
			if (axeSpeed < 0) relativeSpeed -= axeSpeed * axis;
		}

		Vector3 force = walkForce * (axis * headStickSpeed - relativeSpeed);

		if (!standing) force *= airWalkCoefficient;

		Vector3 realForce = new Vector3(force.x, 0, force.z) * body.mass / 10f;
		body.AddForce(realForce);

		appliedForce += realForce;

		walking = axis.sqrMagnitude > 0.001f && standing;

	}
    */
		
}
