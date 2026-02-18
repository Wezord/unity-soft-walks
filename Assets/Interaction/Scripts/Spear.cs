using UnityEngine;

public class Spear : MonoBehaviour
{
    public float thrustLength = 1.0f;
    public double thrustDuration = 0.2;

	//public AK.Wwise.Event attackSoundEvent;

	private ConfigurableJoint joint;
    private GenericPlayerInput input;
    private Rigidbody body;
    private Grabber grab;

    private bool pulling = false;

    private double lastThrustTime = 0;

    void Start()
    {
        input = GenericPlayerInput.GetInput(gameObject);
        joint = GetComponent<ConfigurableJoint>();
        body = GetComponent<Rigidbody>();
        grab = GetComponent<Grabber>();
    }

    void FixedUpdate()
    {
        var tp = joint.targetPosition;

        grab.SetGrabbing(input.GetSecondaryActionButton());
        
        if (input.GetPrimaryActionButton())
        {
            pulling = true;
            tp.y = thrustLength;
        } else if(pulling)
        {
            pulling = false;
            tp.y = -thrustLength;
            lastThrustTime = Time.timeAsDouble;
            //attackSoundEvent.Post(gameObject);
		} else if(Time.timeAsDouble > lastThrustTime + thrustDuration)
        {
            tp.y = 0;
        }

        joint.targetPosition = tp;
    }


}
