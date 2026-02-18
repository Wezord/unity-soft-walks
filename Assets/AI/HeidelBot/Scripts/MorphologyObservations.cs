using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class MorphologyObservations : MonoBehaviour
{
    private VectorSensorComponent nogailSensor;
    private HumanBotParameters p;
    private CruralIndex crural;

    void Start()
    {
        var events = GetComponent<AgentEvents>();

        events.collectObservationsEvent += CollectObservations;

        nogailSensor = GetComponent<VectorSensorComponent>();

        crural = GetComponent<CruralIndex>();

        p = GetComponent<HumanBotParameters>();
    }

    private void CollectObservations(VectorSensor _)
    {
        var sensor = nogailSensor.GetSensor();

        sensor.AddObservation(p.height);
		sensor.AddObservation(p.totalMass);
		sensor.AddObservation(crural.currentCruralIndex);

	}
}
