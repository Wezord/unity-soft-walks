using Unity.MLAgents.Sensors;
using UnityEngine;

public abstract class JointController : MonoBehaviour
{
	public BotJoint joint;
	public Transform pointer;
	public Transform target;
	public Transform targetCenter;
	public Transform targetReference;
	public float targetRadius = 1f;
	public bool randomTarget;
	public int targetFrames = 100;
	public int targetFrameOffset = 0;
	public float reward = 1f;

	public bool horizontal = false;

	protected BotParameters p;
	protected BotAgent agent;


	protected VectorSensorComponent nogailSensorComponent;

	protected TargetMover targetMover;
	protected int targetCounter = 0;

	protected Vector3 targetDiff;

	protected void Awake()
	{
		p = GetComponent<HumanBotParameters>();
		agent = GetComponent<HumanBotAgent>();

		AgentEvents events = GetComponent<AgentEvents>();
		events.beginEpisodeEvent += OnEpisodeBegin;
		events.collectObservationsEvent += CollectObservations;

		targetMover = target.GetComponent<TargetMover>();
		if (targetCenter == null) targetCenter = joint.transform;

		UpdateObjective();

		nogailSensorComponent = GetComponent<VectorSensorComponent>();
	}

	protected void OnEpisodeBegin()
	{
		targetCounter = targetFrameOffset;
		NewTarget();
		UpdateTarget();
		UpdateObjective();
	}

	protected void CollectObservations(VectorSensor sensor)
	{

		if (p.training && agent.decisionCounter != 0)
		{
			AddRewards();
		}

		targetCounter+=p.decisionPeriod;
		if (targetCounter > targetFrames)
		{
			targetCounter = 0;
			NewTarget();
		}
		UpdateTarget();

		UpdateObjective();

		UpdateObservations(sensor);

	}

	protected abstract void AddRewards();

	protected abstract void UpdateObservations(VectorSensor sensor);

	protected void UpdateTarget()
	{
		if (targetMover)
		{
			target.position = targetReference.position + targetDiff;
		}
	}

	protected virtual void NewTarget()
	{
		if (targetMover)
		{

			if (agent.Randomizing() && randomTarget)
			{
				targetDiff = Horizontal(Random.insideUnitSphere) * targetRadius + targetCenter.position - targetReference.position;
			}
			else
			{
				targetDiff = p.target.position - targetReference.position;
			}
		}
	}

	protected abstract void UpdateObjective();

	public Vector3 Horizontal(Vector3 v)
	{
		if(horizontal) v.y = 0;
		return v;
	}
}
