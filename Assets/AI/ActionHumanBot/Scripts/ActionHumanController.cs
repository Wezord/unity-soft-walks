using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class ActionHumanController : MonoBehaviour
{

    public HumanBotParameters p;

    private ActionAgent agent;
    private AgentEvents actionEvents, humanEvents;

    void Awake()
    {
        agent = GetComponent<ActionAgent>();

		if (agent.training)
		{
			Physics.IgnoreLayerCollision(p.gameObject.layer, p.gameObject.layer, true);
		}

		if (agent.recording) return;

		actionEvents = GetComponent<AgentEvents>();
		humanEvents = p.GetComponent<AgentEvents>();
		actionEvents.collectObservationsEvent += ActionCollectObservations;
		actionEvents.endEpisodeEvent += ActionEndEpisode;
		humanEvents.endEpisodeEvent += HumanEndEpisode;

		p.target = agent.head;

	}

    public void ActionCollectObservations(VectorSensor sensor)
    {
		agent.head.position = p.head.transform.position;
	}

    public void ActionEndEpisode()
    {
        p.agent.EndEpisode();
    }

	public void HumanEndEpisode()
	{
		agent.EndEpisode();
	}

}
