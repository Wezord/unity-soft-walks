using System;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;


using static Unity.MLAgents.StatAggregationMethod;

/* 
 * Sees the InterestInfo components and is rewarded when they get near
 * 
 */

public class TargetLocationsGoal : MonoBehaviour
{

	public Transform[] interests = new Transform[0];
	public float interestRadius = 1.5f;
	public float winReward = 1f;

	private ActionAgent agent;

	private bool[] hasAttainedInterests;
	private float[] timeAttainedInterests;

	private BufferSensorComponent interestSensor;

	private AgentEvents events;

	void Start()
    {
		agent = GetComponent<ActionAgent>();

		interestSensor = GetComponent<BufferSensorComponent>();

		hasAttainedInterests = new bool[interests.Length];
		timeAttainedInterests = new float[interests.Length];

		events = GetComponent<AgentEvents>();

		events.beginEpisodeEvent += OnEpisodeBegin;
		events.collectObservationsEvent += CollectObservations;
		events.actionReceivedEvent += OnActionReceived;
		events.endEpisodeEvent += OnEpisodeEnd;
	}

	private void OnEpisodeEnd()
	{
		for (int i = 0; i < hasAttainedInterests.Length; i++)
		{
			agent.RecordStat("Interests/Interest " + i, hasAttainedInterests[i] ? 1f : 0f);
		}
	}

	private void OnEpisodeBegin()
	{
		// This is wonky, what happens if the length changes ?
		if(hasAttainedInterests.Length != interests.Length || timeAttainedInterests.Length != interests.Length)
		{
			hasAttainedInterests = new bool[interests.Length];
			timeAttainedInterests = new float[interests.Length];
		} else
		{
			Array.Clear(hasAttainedInterests, 0, hasAttainedInterests.Length);
			Array.Clear(timeAttainedInterests, 0, timeAttainedInterests.Length);
		}
	}

	private void AddRewards()
	{
		for (int i = 0; i < interests.Length; i++)
		{
			Transform interest = interests[i];
			if (hasAttainedInterests[i])
			{
				continue;
			}

			Vector3 diff = agent.head.position - interest.position;
			diff.y = 0;
			if (diff.sqrMagnitude < interestRadius * interestRadius)
			{
				hasAttainedInterests[i] = true;
				timeAttainedInterests[i] = Time.time;
				agent.AddReward(winReward);
				agent.RecordStat("Interests/Interest " + i + " frame", agent.frameCounter, Histogram);
			}
		}
	}

	private void CollectObservations(VectorSensor sensor)
	{
		AddRewards();

		float[] obs = new float[interestSensor.ObservableSize];

		for (int i = 0; i < interests.Length; i++)
		{
			Array.Clear(obs, 0, obs.Length);
			var interest = interests[i];
			var index = 0;

			var pos = agent.referential.InverseTransformPoint(interest.position);
			obs[index++] = pos.x;
			obs[index++] = pos.y;
			obs[index++] = pos.z;

			var timeSinceAttained = (Time.time - timeAttainedInterests[i]) / (agent.maxMaxFrames * Time.fixedDeltaTime);
			if (timeAttainedInterests[i] == 0) timeSinceAttained = -1f;
			obs[index++] = timeSinceAttained;

			var info = interest.GetComponent<InterestInfo>();
            if (info)
            {
				obs[index + 0] = info.color.x;
				obs[index + 1] = info.color.y;
				obs[index + 2] = info.color.z;
			}

			interestSensor.AppendObservation(obs);
		}


	}

	private void OnActionReceived(ActionBuffers action, ref int i)
	{
		
	}
}
