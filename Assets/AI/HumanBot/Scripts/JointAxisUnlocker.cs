using System;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class JointAxisUnlocker : MonoBehaviour
{

	[Serializable]
	public struct JointAxes
	{
		public BotJoint joint;
		public bool NX, NY, NZ, TX, TY, TZ, VX, VY, VZ;
	}

	[SerializeField]
	public JointAxes[] joints;

	private VectorSensorComponent nogailSensorComponent;

	private VectorSensor sensor, nogailSensor;

	protected void Awake()
	{
		nogailSensorComponent = GetComponent<VectorSensorComponent>();

		foreach (var j in joints)
		{
			var joint = j.joint;
			joint.passVelocityObs = false;
			joint.passNormalObs = false;
			joint.passTangentObs = false;
		}

		AgentEvents events = GetComponent<AgentEvents>();
		events.collectObservationsEvent += CollectObservations;
	}

	protected void CollectObservations(VectorSensor sensor)
	{
		this.sensor = sensor;
		this.nogailSensor = nogailSensorComponent.GetSensor();

		foreach(var j in joints)
		{
			var joint = j.joint;
			GetSensor(j.NX).AddObservation(joint.normalObs.x);
			GetSensor(j.NY).AddObservation(joint.normalObs.y);
			GetSensor(j.NZ).AddObservation(joint.normalObs.z);
			GetSensor(j.TX).AddObservation(joint.tangentObs.x);
			GetSensor(j.TY).AddObservation(joint.tangentObs.y);
			GetSensor(j.TZ).AddObservation(joint.tangentObs.z);
			GetSensor(j.VX).AddObservation(joint.velocityObs.x);
			GetSensor(j.VY).AddObservation(joint.velocityObs.y);
			GetSensor(j.VZ).AddObservation(joint.velocityObs.z);
		}

	}

	private VectorSensor GetSensor(bool nogail)
	{
		return nogail ? nogailSensor : sensor;
	}
}
