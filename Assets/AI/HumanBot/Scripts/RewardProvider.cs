using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

public abstract class RewardProvider : MonoBehaviour
{
	public abstract void AddRewards(HumanBotAgent agent, HumanBotParameters p);

	private void Callback(VectorSensor _)
	{
		var agent = GetComponent<HumanBotAgent>();
		var p = GetComponent<HumanBotParameters>();

		AddRewards(agent, p);
	}

	public void OnEnable()
	{
		var events = GetComponent<AgentEvents>();
		// Connect the event directly to the callback while ignoring the VectorSensor
		events.collectObservationsEvent += Callback;
	}

	public void OnDisable()
	{
		var events = GetComponent<AgentEvents>();
		// Disconnect the event
		events.collectObservationsEvent -= Callback;
	}
}
