using Unity.MLAgents.Sensors;
using UnityEngine;

using static Unity.MLAgents.StatAggregationMethod;

public class DirectionController : JointController
{

	private Vector3 targetDirection;

	protected override void AddRewards()
	{
		float overlap = Vector3.Dot(targetDirection, Horizontal(pointer.transform.forward).normalized);

		float reward = overlap * this.reward;

		agent.RecordStat("Direction/" + pointer.name + " Direction Overlap", overlap, Histogram);
		agent.RecordStat("Direction/" + pointer.name + " Direction Reward", reward, Histogram);

		agent.AddReward(reward);
	}

	protected override void UpdateObservations(VectorSensor sensor)
	{
		if (p.recording)
		{
			joint.normalObs = Vector3.zero;
			joint.tangentObs = Vector3.zero;
		} else
		{
			var forwardDiff = Horizontal(targetDirection - pointer.forward);
			joint.normalObs = joint.GetReferential(agent.referential).InverseTransformVector(forwardDiff);
			joint.tangentObs = Vector3.zero;
		}
	}

	protected override void UpdateObjective()
	{
		targetDirection = Horizontal(target.position - pointer.position).normalized;
	}
}
