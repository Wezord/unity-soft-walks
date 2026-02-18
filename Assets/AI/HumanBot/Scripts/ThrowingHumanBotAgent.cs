using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class ThrowingHumanBotAgent : HumanBotAgent
{
    [SerializeField] private Rigidbody hand;
    public Rigidbody spear;

    private bool hasThrown = false;

    public float velocityReward = 1;

    public override void CollectGoalObservations(VectorSensor sensor)
    {
        base.CollectGoalObservations(sensor);

        sensor.AddObservation(referential.InverseTransformVector(spear.velocity));
        sensor.AddObservation(referential.InverseTransformPoint(hand.position));

        //Target (x,y,z) 3 positons
        sensor.AddObservation(Vector3.zero);

        // Vecteur spear - hand pour connaître la direction (x,y) 2 positions
        sensor.AddObservation(Vector2.zero);

    }

    public override void CalculateRewards()
    {
        base.CalculateRewards();
        
    }


    public override void OnActionReceived(ActionBuffers actions)
    {
        base.OnActionReceived(actions);
        int throwAction = actions.DiscreteActions[0];

        /*
        if (throwAction == 1)
        {
            ThrowObject();
        }
        */
    }

    void ThrowObject()
    {
        hasThrown = true;

        /*
        // Cherche le spear
        Transform spearTransform = hand.Find("spear");
        if (spearTransform == null) return;

        GameObject spear = spearTransform.gameObject;
        Rigidbody spearRb = spear.GetComponent<Rigidbody>();
        if (spearRb == null) return;
        */

        // Si le spear est attaché par un joint, on le détache
        ConfigurableJoint joint = spear.GetComponent<ConfigurableJoint>();
        if (joint != null)
        {
            DestroyImmediate(joint); // détache le spear de la main
        }

        // Copie la vitesse de la main
        /*
        if (hand != null)
        {
            spear.velocity = hand.velocity;
            spear.angularVelocity = hand.angularVelocity; // copie aussi la rotation
        }
        */
        
        var speed = velocityReward * spear.velocity.magnitude;

        if(hasThrown) AddReward(speed);

        // Ajoute une force pour lancer le spear vers l'avant
        //float throwSpeed = 1f; // ajustable
        //spearRb.AddForce(hand.forward * throwSpeed, ForceMode.VelocityChange);
    }

    public override void OnEpisodeBegin() {
        base.OnEpisodeBegin();

        spear.GetComponent<Resetter>().ResetTransformRecursive();

        hasThrown = false;

        SetSpearJoint();
        
    }

    public void SetSpearJoint()
    {
        // Readd conf joint
        ConfigurableJoint joint = spear.GetComponent<ConfigurableJoint>();
        if (joint != null)
        {
            DestroyImmediate(joint);
        }

        if(p.recording || p.animating) return;

        joint = spear.gameObject.AddComponent<ConfigurableJoint>();

        joint.connectedBody = hand;
        
        joint.xMotion = joint.yMotion = joint.zMotion = joint.angularXMotion = joint.angularXMotion = joint.angularXMotion = joint.angularXMotion = ConfigurableJointMotion.Locked;

    }

}
