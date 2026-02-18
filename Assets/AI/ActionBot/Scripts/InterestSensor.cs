using System;
using System.Collections.Generic;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class InterestSensor : MonoBehaviour
{

	public List<Transform> interests;

	private BufferSensorComponent sensor;

	private ActionAgent agent;

	void Start()
	{
		agent = GetComponent<ActionAgent>();

		sensor = GetComponent<BufferSensorComponent>();

		var events = GetComponent<AgentEvents>();
		events.collectObservationsEvent += CollectObservations;
	}

	private void CollectObservations(VectorSensor _)
	{
		float[] obs = new float[sensor.ObservableSize];

		for (int i = 0; i < interests.Count; i++)
		{
			Array.Clear(obs, 0, obs.Length);
			var target = interests[i];
			var index = 0;

			var pos = agent.referential.InverseTransformPoint(target.position);
			obs[index++] = pos.x;
			obs[index++] = pos.y;
			obs[index++] = pos.z;

			var info = target.GetComponent<InterestInfo>();
			if (info)
			{
				obs[index + 0] = info.color.x;
				obs[index + 1] = info.color.y;
				obs[index + 2] = info.color.z;
			}

			sensor.AppendObservation(obs);
		}
	}

}
