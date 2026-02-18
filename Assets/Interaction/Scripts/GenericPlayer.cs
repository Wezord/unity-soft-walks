using System;
using UnityEditor;
using UnityEngine;

public abstract class GenericPlayer : MonoBehaviour
{

    public Body capsule;

    public new GameObject camera;

    [HideInInspector]
    public Rigidbody body;

    protected GenericPlayerInput input;

    protected Vector3 appliedForce = Vector3.zero;

    protected Vector3 groundSpeed = Vector3.zero;

    [Header("Stand Movement")]
    public float playerHeight = 1.8f;

    public float legForce = 1000f;
    public float standForce = 1000f;
    public float standSpeed = 5f;
    public float jumpForce = 1000f;
    public float jumpSpeed = 10f;

	public LayerMask standLayers, moveLayers;

	public bool standing = false;
    public bool colliding = false;

    [NonSerialized]
    public RaycastHit standingOn;


    [Header("Walk Movement")]
    [Min(0)] public float walkForce = 1.0f;

    [Min(0)] public float walkSpeed = 5.0f;

    [Min(0)] public float sprintSpeed = 1.0f;

    [Min(0)] public float airWalkCoefficient = 0f;

    [Range(0, 1)]
    public float linearDrag = 0.90f;

    public Transform movementAxis;

    protected bool initialized = false;

    protected bool walking = false;

    private void Awake()
    {
        Initialize();
    }

    protected virtual void Initialize()
    {
        if (initialized)
        {
            return;
        }
        initialized = true;

        body = GetComponent<Rigidbody>();

        input = GenericPlayerInput.GetInput(gameObject);

        capsule.Initialize(this);
    }

    protected virtual void FixedUpdate()
    {
        appliedForce = Vector3.zero;

        Stand();

        Walk();

        Jump();

        //ApplyForcesToSupport();

        ApplySounds();

	}

    protected void Stand()
    {
        float height = GetHeight();

		capsule.SetHeight(height);

		standing = false;

		groundSpeed = Vector3.zero;

        if (Physics.Raycast(camera.transform.position, Vector3.down, out standingOn, height + 0.2f, standLayers))
        {

		    standing = true;

            capsule.SetHeight(Mathf.Min(standingOn.distance, height));

			if (standingOn.rigidbody && standingOn.rigidbody.GetComponent<Grabbed>())
			{
				return;
			}

			if (moveLayers == (moveLayers | (1 << standingOn.transform.gameObject.layer)))
            {
				if (standingOn.rigidbody) groundSpeed = standingOn.rigidbody.GetPointVelocity(standingOn.point);
				else if (standingOn.articulationBody) groundSpeed = standingOn.articulationBody.GetPointVelocity(standingOn.point);
			}

            float force = standForce * (standSpeed * 2f*Mathf.Min(0.5f, (height - standingOn.distance)) + groundSpeed.y - body.velocity.y);

            force = Mathf.Min(force, legForce);

            if (!float.IsNaN(force) && force > 0)
            {
                Vector3 realForce = new Vector3(0, force, 0) * body.mass / 10f;

                body.AddForce(realForce);

                appliedForce += realForce;
            }
        }

    }

    protected void Jump()
    {
        if (standing && input.GetJumpButton())
        {
            float force = jumpForce * (jumpSpeed + groundSpeed.y - body.velocity.y);
            force = Mathf.Min(force, legForce);
            Vector3 realForce = new Vector3(0, force, 0) * body.mass / 10f;
            body.AddForce(realForce);

            appliedForce += realForce;

			//jumpSoundEvent.Post(gameObject);
		}
    }

    virtual protected void Walk()
    {
        Vector2 axis = input.GetMovementAxis();

        Vector3 direction = movementAxis.forward;
        Vector3 normal = new Vector3(direction.x, 0, direction.z).normalized;
        Vector3 tangent = new Vector3(normal.z, 0, -normal.x);
        Vector3 axe = (axis.x * tangent + axis.y * normal);

        float magnitude = input.GetWalkingButton() ? sprintSpeed : walkSpeed;

        Vector3 relativeSpeed = body.velocity - groundSpeed;

        if (axe.sqrMagnitude != 0)
        {
            float axeSpeed = Vector3.Dot(relativeSpeed, axe);
            if (axeSpeed < 0) relativeSpeed -= axeSpeed * axe;
        }

        Vector3 force = walkForce * (axe * magnitude - relativeSpeed);

        if (!standing) force *= airWalkCoefficient;

        Vector3 realForce = new Vector3(force.x, 0, force.z) * body.mass / 10f;

        body.AddForce(realForce);

        appliedForce += realForce;

        walking = axe.sqrMagnitude > 0.001f && standing;

    }

    protected void ApplyForcesToSupport()
    {
        if (standingOn.rigidbody) standingOn.rigidbody.AddForceAtPosition(-appliedForce, standingOn.point);
        else if (standingOn.articulationBody) standingOn.articulationBody.AddForceAtPosition(-appliedForce, standingOn.point);
    }

    public abstract float GetHeight();

	//public AK.Wwise.Event walkSoundEvent;
	//public AK.Wwise.Event runSoundEvent;
	private uint walkSoundID = 0;
    private bool lastRunning = false;

	//public AK.Wwise.Event jumpSoundEvent;
	//public AK.Wwise.Event fallSoundEvent;
	private bool lastStanding = false;

	//public AK.Wwise.RTPC relativeVelocityRTPC;

    private void ApplySounds()
    {
		if (walking)
		{

			bool running = input.GetWalkingButton();
			// start if not started
			
            lastRunning = running;

		}
		else
		{
			// stop if started
            //if(walkSoundID>0) AkSoundEngine.StopPlayingID(walkSoundID);
            walkSoundID = 0;
		}

        if(standing && !lastStanding)
		{
            float relativeVelocity = Vector3.Dot(Vector3.down, body.velocity - groundSpeed);
			//relativeVelocityRTPC.SetValue(gameObject, relativeVelocity);
			//fallSoundEvent.Post(gameObject);
		}
        lastStanding = standing;

	}

}