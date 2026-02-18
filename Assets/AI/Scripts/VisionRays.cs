using Unity.MLAgents.Sensors;
using UnityEngine;

public class VisionRays : MonoBehaviour
{

	public Transform eye;
	public float visionDistance = 10f;
	public LayerMask visionMask;

	private BotParameters p;
	private BotAgent agent;
	private VectorSensorComponent sensorComponent;

	void Awake()
    {
		p = GetComponent<BotParameters>();
		agent = GetComponent<BotAgent>();

		AgentEvents events = GetComponent<AgentEvents>();
		events.collectObservationsEvent += CollectObservations;

		sensorComponent = GetComponent<VectorSensorComponent>();
	}

	public void CollectObservations(VectorSensor _)
	{
		var sensor = sensorComponent.GetSensor();

		if (!p.target) return;

		var direction = BotAgent.Horizontal(p.target.position - eye.position);
		direction = Vector3.RotateTowards(direction, Vector3.down, Mathf.PI / 3f, Mathf.Infinity);
		if(p.targetSpeed == 0)
		{
			direction = Vector3.down;
		}

		//Debug.DrawRay(eye.position, direction * 1000f, Color.blue, p.decisionPeriod * Time.fixedDeltaTime, true);

		Vector3 normal = Vector3.up;
		float distance = -1f;
		if (Physics.Raycast(eye.position, direction, out RaycastHit hitInfo, visionDistance, visionMask))
		{
			normal = hitInfo.normal;
			distance = hitInfo.distance;
			//Debug.DrawRay(hitInfo.point, hitInfo.normal * distance, Color.blue, p.decisionPeriod * Time.fixedDeltaTime, false);
		}
		sensor.AddObservation(agent.referential.InverseTransformVector(normal));
		sensor.AddObservation(distance);
	}
}
