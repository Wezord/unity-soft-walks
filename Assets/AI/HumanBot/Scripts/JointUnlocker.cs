using Unity.MLAgents.Sensors;
using UnityEngine;

public class JointUnlocker : MonoBehaviour
{
	public BotJoint[] joints;

	private VectorSensorComponent nogailSensorComponent;

	protected void Awake()
	{
		nogailSensorComponent = GetComponent<VectorSensorComponent>();

		foreach (var joint in joints)
		{
			joint.passVelocityObs = false;
			joint.passNormalObs = false;
			joint.passTangentObs = false;
		}

		AgentEvents events = GetComponent<AgentEvents>();
		events.collectObservationsEvent += CollectObservations;
	}

	protected void CollectObservations(VectorSensor sensor)
	{
		var nogailSensor = nogailSensorComponent.GetSensor();

		foreach(var joint in joints)
		{
			nogailSensor.AddObservation(joint.velocityObs);
			nogailSensor.AddObservation(joint.normalObs);
			nogailSensor.AddObservation(joint.tangentObs);
		}

	}
}
