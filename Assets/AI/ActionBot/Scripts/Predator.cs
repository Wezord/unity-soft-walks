using System;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class Predator : MonoBehaviour
{


	public List<Transform> targets;
	public float winRadius = 1.5f;
	public float winReward = 1f;

	private ActionAgent agent;

	private BufferSensorComponent targetSensor;

	private AgentEvents events;

	private InterestSensor interestSensor;

	private MoveOnTerrain mover;

	private int winCount = 0;
	private float totalWinReward = 0;

	void Start()
	{
		agent = GetComponent<ActionAgent>();

		targetSensor = GetComponent<BufferSensorComponent>();

		interestSensor = GetComponent<InterestSensor>();

		events = GetComponent<AgentEvents>();

		mover = GetComponent<MoveOnTerrain>();

		events.beginEpisodeEvent += OnEpisodeBegin;
		events.collectObservationsEvent += CollectObservations;
		events.endEpisodeEvent += OnEpisodeEnd;

		// Add preys to targets list
		var prey = FindObjectsOfType<Prey>();
		targets = prey.Select(x => x.transform).ToList();
		interestSensor.interests = targets;

	}

	private void OnEpisodeBegin()
	{
		winCount = 0;

		mover.RandomRespawn();
	}

	private void OnEpisodeEnd()
	{
		agent.RecordStat("Win/Win Count", winCount, StatAggregationMethod.Histogram);
	}


	private void CollectObservations(VectorSensor _)
	{
		AddRewards();
	}

	private void AddRewards()
	{
		for (int i = 0; i < targets.Count; i++)
		{
			Transform target = targets[i];
			
			// Check if target is near enough
			Vector3 diff = agent.head.position - target.position;
			diff.y = 0;
			if (diff.sqrMagnitude < winRadius * winRadius)
			{
				Win();
				var prey = target.GetComponent<Prey>();
				prey.Kill();
			}
		}
	}

	private void Win()
	{
		winCount++;
		var reward = winReward;
		agent.AddReward(winReward);
		agent.RecordStat("Win/Win frame", agent.frameCounter, StatAggregationMethod.Histogram);
	}

}
