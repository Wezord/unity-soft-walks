using System.Linq;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class Prey : MonoBehaviour
{

	public float winReward = 1f, deathPenalty = -1f;

	private ActionAgent agent;
	private InterestSensor interestSensor;
	private AgentEvents events;
	private MoveOnTerrain mover;

	private bool isDead = false;

	void Start()
	{
		agent = GetComponent<ActionAgent>();

		interestSensor = GetComponent<InterestSensor>();

		mover = GetComponent<MoveOnTerrain>();

		events = GetComponent<AgentEvents>();

		events.beginEpisodeEvent += OnEpisodeBegin;
		events.collectObservationsEvent += CollectObservations;
		events.actionReceivedEvent += OnActionReceived;
		events.endEpisodeEvent += OnEpisodeEnd;

		// Add preys to watch list
		var predators = FindObjectsOfType<Predator>();
		var targets = predators.Select(x => x.transform).ToList();
		interestSensor.interests = targets;
	}

	private void OnEpisodeBegin()
	{
		isDead = false;

		mover.RandomRespawn();
	}

	private void OnEpisodeEnd()
	{
		if (!isDead) Win();
		agent.RecordStat("Win/Win", isDead ? 0 : 1);
	}
	

	private void AddRewards()
	{
		
	}

	private void Win()
	{
		agent.AddReward(winReward);
	}
	private void CollectObservations(VectorSensor sensor)
	{
		AddRewards();
	}

	private void OnActionReceived(Unity.MLAgents.Actuators.ActionBuffers action, ref int i)
	{

	}

	public void Kill()
	{
		isDead = true;
		agent.AddReward(deathPenalty);
		agent.OnEpisodeEnd();
		agent.EndEpisode();
	}
}
