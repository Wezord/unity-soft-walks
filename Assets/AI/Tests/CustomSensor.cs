using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;

public class CustomSensor : VectorSensor
{
	public CustomSensor(int observationSize, string name = null, ObservationType observationType = ObservationType.Default) : base(observationSize, name, observationType)
	{
	}

	public List<float> GetObservationData()
	{
		return (List<float>)this.GetType().BaseType.GetField("m_Observations", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(this);
	}

	public static List<string> GetObservationsFromAgent(BotAgent agent)
	{
		var param = agent.GetComponent<BehaviorParameters>();
		CustomSensor sensor = new CustomSensor(param.BrainParameters.VectorObservationSize);
		agent.CollectObservations(sensor);
		return sensor.GetObservationData().ConvertAll((f) => f.ToString());
	}

}
