using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class RollingBotAgent : FlyingBotAgent
{

    public override void CollectObservations(VectorSensor sensor)
    {

        Vector3 error = calculateError();

        sensor.AddObservation(error);

        sensor.AddObservation(body.velocity);

        sensor.AddObservation(body.angularVelocity);

        float reward = -error.magnitude / 1000f;

        AddReward(reward);

        lastError = error;

    }

    public override void OnActionReceived(ActionBuffers action, ref int i)
    {

        body.maxAngularVelocity = 30;

        Vector3 signal = Vector3.zero;
        signal.x = action.ContinuousActions[0];
        //signal.y = action.ContinuousActions[1];
        signal.z = action.ContinuousActions[2];

        Vector3 torque = signal * p.maxTorque;

        body.AddTorque(torque);

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {

    }

    public override void SetTarget()
    {

        base.SetTarget();

        if (p.targetMover) p.target.position = new Vector3(p.target.position.x, 0, p.target.position.z);

    }

}