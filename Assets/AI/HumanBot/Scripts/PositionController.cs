using Unity.MLAgents.Sensors;
using UnityEngine;

using static Unity.MLAgents.StatAggregationMethod;

public class PositionController : JointController
{
	private Vector3 targetDirection;
	private float lastDistance;

	protected override void AddRewards()
	{
		UpdateTarget();

		float distance = Horizontal(pointer.position - target.position).magnitude;
		float movement = (lastDistance - distance) / (p.decisionPeriod * Time.fixedDeltaTime);

		float reward = movement * this.reward;

		agent.RecordStat("Control/" + pointer.name + " Position Error", distance, Histogram);
		agent.RecordStat("Control/" + pointer.name + " Position Movement", movement, Histogram);
		agent.RecordStat("Control/" + pointer.name + " Position Reward", reward, Histogram);

		agent.AddReward(reward);
	}

	protected override void UpdateObservations(VectorSensor sensor)
	{
		if (p.recording)
		{
			joint.velocityObs = Vector3.zero;
		} else
		{
			var diff = targetDirection;
			joint.velocityObs = joint.GetReferential(agent.referential).InverseTransformVector(diff);
			//joint.passVelocityObs = true;
		}

		if (nogailSensorComponent && nogailSensorComponent.GetSensor() != null)
		{
			var nogailSensor = nogailSensorComponent.GetSensor();
			nogailSensor.AddObservation(agent.referential.InverseTransformPoint(target.position));
			nogailSensor.AddObservation(agent.referential.InverseTransformPoint(pointer.position));
		}
	}

	protected override void UpdateObjective()
	{
		targetDirection = Horizontal(target.position - pointer.position);
		lastDistance = targetDirection.magnitude;
	}
}
