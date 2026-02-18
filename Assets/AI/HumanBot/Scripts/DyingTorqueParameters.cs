using UnityEngine;

public class DyingTorqueParameters : MonoBehaviour
{
    public float torqueFactor = 1f, springFactor = 1f, damperFactor = 1f;
    public float grabbedTorqueFactor = 1f, grabbedSpringFactor = 1f, grabbedDamperFactor = 1f;
    private float originalTorqueFactor, originalSpringFactor, originalDamperFactor;

    private BotJoint joint;
    private HumanBotAgent agent;

    private int grabCount = 0;
    private bool grabbed = false;

    public void SetGrabbing(bool grabbed)
    {
        if(grabbed == this.grabbed) return;
        this.grabbed = grabbed;
        joint.isFrozen = grabbed;
    }

	void Start()
    {
        joint = GetComponent<BotJoint>();
        agent = (HumanBotAgent) joint.p.agent;

        originalTorqueFactor = joint.torqueFactor;
        originalSpringFactor = joint.springFactor;
        originalDamperFactor = joint.damperFactor;


    }

    void FixedUpdate()
    {
        if(grabbed) {
            joint.torqueFactor = grabbedTorqueFactor;
            joint.springFactor = grabbedSpringFactor;
            joint.damperFactor = grabbedDamperFactor;
        } else if (agent.IsDying())
        {
            joint.torqueFactor = torqueFactor;
            joint.springFactor = springFactor;
            joint.damperFactor = damperFactor;
        }  else {
            joint.torqueFactor = originalTorqueFactor;
            joint.springFactor = originalSpringFactor;
            joint.damperFactor = originalDamperFactor;
        }
    }

    private void OnGrab(GameObject grabber, GameObject obj, PlayerType type)
    {
        if (obj == gameObject || grabber == gameObject)
        {
            grabCount++;
            if(grabCount > 0)
                SetGrabbing(true);

            // Call grab on parent
            var joint = GetComponent<ConfigurableJoint>();
            if(joint.connectedBody == null)  return;
            var param = joint.connectedBody.GetComponent<DyingTorqueParameters>();
            if(param== null) return;
            param.OnGrab(grabber, joint.connectedBody.gameObject, type);
            
        }
    }
    private void OnUnGrab(GameObject grabber, GameObject obj, PlayerType type)
    {
        if (obj == gameObject || grabber == gameObject)
        {
            grabCount--;
            if (grabCount <= 0)
                SetGrabbing(false);
            
            // Call ungrab on parent
            var joint = GetComponent<ConfigurableJoint>();
            if(joint.connectedBody == null)  return;
            var param = joint.connectedBody.GetComponent<DyingTorqueParameters>();
            if(param== null) return;
            param.OnUnGrab(grabber, joint.connectedBody.gameObject, type);
            
        }
    }

    void OnEnable()
    {
        Events.grabEvent += OnGrab;
        Events.ungrabEvent += OnUnGrab;
    }

}
