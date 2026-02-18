using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class FlyingBotAgent : BotAgent
{

    private ConfigurableJoint joint;

    public const float maxErrorMultiplier = 5;
    public const float maxVelocityMultiplier = 1;
    public const float maxForce = 10;
    public const float targetVelocityErrorMultiplier = 3;

	public override void Initialize()
    {
        base.Initialize();

        body = GetComponent<Rigidbody>();

        joint = GetComponent<ConfigurableJoint>();

    }


    protected Vector3 calculateError()
    {
        return p.target.position - transform.position;
    }


    public override void OnEpisodeBegin()
    {

        if (p.training)
        {
            transform.position = startPosition;
            body.velocity = new Vector3();

            frameCounter = 0;
        }

        SetTarget();

    }

	protected new void FixedUpdate()
    {
        if (Vector3.Distance(transform.position, p.target.position) < 0.5)
        {
            SetTarget();
            AddReward(10);
        }

        base.FixedUpdate();

    }

    protected Vector3 lastError;

    public override void CollectObservations(VectorSensor sensor)
    {

        Vector3 error = calculateError();

        sensor.AddObservation(error.magnitude);

        sensor.AddObservation(body.velocity.magnitude);

        float reward = -error.magnitude / 1000f;

        AddReward(reward);

        lastError = error;

    }

    public override void OnActionReceived(ActionBuffers action, ref int i)
    {
        base.OnActionReceived(action, ref i);

        Vector3 error = calculateError();

        Vector3 signal = Vector3.zero;
        signal.x = action.ContinuousActions[0];
        signal.y = action.ContinuousActions[1];

        
        JointDrive drive = new JointDrive();
        drive.positionSpring = (signal.x + 1) / 2 * maxErrorMultiplier;
        drive.positionDamper = (signal.y + 1) / 2 * maxVelocityMultiplier;
        drive.maximumForce = maxForce;
        

        this.joint.xDrive = drive;
        this.joint.yDrive = drive;
        this.joint.zDrive = drive;

        //joint.configuredInWorldSpace = true;
        joint.targetPosition = -transform.InverseTransformPoint(p.target.position);
        joint.targetVelocity = -error.normalized * targetVelocityErrorMultiplier;

        //Vector3 force = (signal.x + 1) / 2 * maxErrorMultiplier * error - (signal.y + 1)/2 * maxVelocityMultiplier * body.velocity;

        //body.AddForce(force);

    }

    public override void SetTarget()
    {
        
        if (p.targetMover) p.targetMover.randomRadius(transform.position, 2, 4);
        
    }

}