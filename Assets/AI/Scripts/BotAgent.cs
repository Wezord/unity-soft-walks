using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Demonstrations;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;

public abstract class BotAgent : Agent
{

	[System.NonSerialized]
	public BotParameters p;

	[System.NonSerialized]
	public Rigidbody body;

	protected RigidbodyCopy copy;

	[System.NonSerialized]
	public StatsRecorder statsRecorder;

	protected int frameCounter = 0;
	[System.NonSerialized]
	public int decisionCounter = 0;
	protected Vector3 startPosition;
	protected bool initialized = false;

	[System.NonSerialized]
	public float stepCollisionPenalty = 0;

	[System.NonSerialized]
	public Referential referential;


	public static int currentDecisionFrame = 0;
	private int decisionFrame = 0;

	private AgentEvents events;

	protected override void Awake()
    {
		base.Awake();

        var seed = Academy.Instance.EnvironmentParameters.GetWithDefault("seed", -1);
		if(seed != -1) {
			UnityEngine.Random.InitState((int)seed);
		}
    }

	public override void Initialize()
    {
		base.Initialize();

		p = GetComponent<BotParameters>();

		p.agent = this;

		UpdateParameters();

		if (p.target == null)
		{
			var target = new GameObject(gameObject.name + " target");
			p.target = target.transform;
			p.targetMover = target.AddComponent<TargetMover>();
		}
		p.targetMover = p.target ? p.target.GetComponent<TargetMover>() : null;
        if (p.training)
        {
			p.breakForce = float.PositiveInfinity;
			p.breakTorque = float.PositiveInfinity;
		}
		currentDecisionFrame = (currentDecisionFrame + 1) % p.decisionPeriod;
		decisionFrame = p.training ? 0 : currentDecisionFrame;
		Academy.Instance.AgentPreStep += RequestDecisions;

		body = GetComponent<Rigidbody>();
		copy = new RigidbodyCopy(transform, body);
		statsRecorder = p.training ? Academy.Instance.StatsRecorder : null;
		body.mass = p.totalMass * p.bodyPercentage / 100f;
		body.maxAngularVelocity = p.maxAngularVelocity;
		startPosition = transform.position;

		events = GetComponent<AgentEvents>();
	}


    public override void OnEpisodeBegin()
    {
		UpdateParameters();

		frameCounter = 0;
		decisionCounter = 0;
		if (p.environment) p.environment.OnEpisodeBegin();

		Restart();
		SetTarget();
		//PointTowardsTarget();

		if (Randomizing())
		{
			if(p.randomTarget) RandomRotation();
			if(p.randomInit && Random.value <= p.randomInitChance) Randomize();
			if (p.randomStartInit)
			{
				{
					// Choisit un XZ aléatoire, raycast vers le bas pour trouver le sol et place l'agent à 0.1f au-dessus
					Vector3 probePos = new Vector3(
						Random.Range(100f, 1300f),
						50f,
						Random.Range(100f, 900f)
					);

					// Détermine la position candidate puis la clamp pour éviter les coordonnées négatives
					Vector3 candidatePos;
					if (Physics.Raycast(probePos, Vector3.down, out RaycastHit hit, 100f))
					{
						candidatePos = hit.point + Vector3.up * 0.2f;
					}
					else
					{
						// fallback si aucun sol détecté
						candidatePos = new Vector3(probePos.x, 0.2f, probePos.z);
					}

					// Empêcher X/Z négatifs et impose une hauteur minimale
					candidatePos.x = Mathf.Max(candidatePos.x, 0f);
					candidatePos.z = Mathf.Max(candidatePos.z, 0f);
					candidatePos.y = Mathf.Max(candidatePos.y, 0.2f);

					Debug.Log("Random start init at position " + candidatePos);

					transform.position = candidatePos;

					// Synchronise le Rigidbody si présent pour éviter des vitesses résiduelles
					if (body != null)
					{
						body.position = transform.position;
						body.velocity = Vector3.zero;
						body.angularVelocity = Vector3.zero;
					}
				}

			}
		}

		if (!p.training || p.animating)
		{
			//MaxStep = 0;
			p.maxFrames = 0;
		}

		events.SignalBeginEpisode();

	}

    protected virtual void FixedUpdate()
    {

        frameCounter++;

        if (p.maxFrames > 0 && frameCounter>p.maxFrames)
        {
			OnEpisodeEnd();
            EpisodeInterrupted();
        }

    }

	public virtual void OnEpisodeEnd()
	{
		events.SignalEndEpisode();

		Events.SignalDestroy(gameObject);
	}

    public void RequestDecisions(int academyStepCount)
    {
		if (academyStepCount % p.decisionPeriod == decisionFrame)
		{
			RequestDecision();
		}
	}

	public override void CollectObservations(VectorSensor sensor)
	{
		p.agent.RecordStat("Collision/Step Collision Penalty", stepCollisionPenalty, StatAggregationMethod.Histogram);
		stepCollisionPenalty = 0;

		events.SignalCollectObservations(sensor);
	}

	public override void OnActionReceived(ActionBuffers action)
	{
		int i = 0;
		OnActionReceived(action, ref i);
	}

	public virtual void OnActionReceived(ActionBuffers action, ref int i)
	{
		events.SignalActionReceived(action, ref i);
		decisionCounter++;
	}

	public override void Heuristic(in ActionBuffers actionsOut)
    {

    }

	public new void AddReward(float reward)
	{
		if (p.training)
		{
			base.AddReward(reward*p.rewardFactor);
		}
	}


	public abstract void SetTarget();

    public virtual void Restart()
    {
		copy.paste(transform, body);
		if (!body.isKinematic)
		{
			body.velocity = Vector3.zero;
			body.angularVelocity = Vector3.zero;
		}
	}

	public virtual void UpdateParameters()
	{
		if (p.training)
		{
			UpdateParameter("rotationSpring", ref p.rotationSpring);
			UpdateParameter("rotationDamper", ref p.rotationDamper);
			UpdateParameter("maxTorque", ref p.maxTorque);
			UpdateParameter("maxAngularVelocity", ref p.maxAngularVelocity);
			UpdateParameter("angularVelocityFactor", ref p.angularVelocityFactor);
			UpdateParameter("totalMass", ref p.totalMass);
			UpdateParameter("energyPenalty", ref p.energyPenalty);
			UpdateParameter("targetSpeed", ref p.targetSpeed);
		}
	}

	public void UpdateParameter(string name, ref float param)
	{
		param = Academy.Instance.EnvironmentParameters.GetWithDefault(name, param);
	}

	public bool Randomizing()
	{
		float randomize = Academy.Instance.EnvironmentParameters.GetWithDefault("randomize", p.randomizing ? 1f : 0f);
		return randomize > 0f && p.training && !p.recording && !p.animating;
	}
	public void PointTowardsTarget()
	{
		Vector3 targetPosition = p.target.position;
		targetPosition.y = transform.position.y;
		transform.LookAt(targetPosition, Vector3.up);
	}

	public void Randomize()
	{
		transform.position += Vector3.up * 2;
		body.velocity = Vector3.down * 4;
		transform.rotation = Random.rotationUniform;
	}

	public void RandomRotation()
	{
		transform.Rotate(Vector3.up, Random.Range(0f, 360f));
	}

	public virtual void Push()
	{
		float strength = 20f;
		Vector3 force = Random.insideUnitSphere * strength;
		Vector3 torque = Random.insideUnitSphere * strength;
		Vector3 position = transform.position;
		body.AddForceAtPosition(force, position, ForceMode.Impulse);
		body.AddTorque(torque, ForceMode.Impulse);
	}


	public virtual void SetKinematic(bool kinematic = true)
	{
		p.kinematic = kinematic;
		body.isKinematic = kinematic;
	}

	public void SetAnimating(bool animating)
	{
		p.animating = animating;
		if (animating) GetComponent<BehaviorParameters>().BehaviorType = BehaviorType.HeuristicOnly;
		SetKinematic(animating);
		if (animating)
		{
			p.animator.enabled = true;
		}
		else if (p.animator)
		{
			p.animator.enabled = false;
		}
	}

	public void SetRecording(bool recording)
	{
		p.recording = recording;
		DemonstrationRecorder recorder = GetComponent<DemonstrationRecorder>();
		if (recording)
		{
			recorder.Record = true;

			var position = Vector3.zero;
			position.z -= UnityEngine.Random.Range(0f, 1f);
			transform.Translate(position);
		}
		else if (recorder)
		{
			recorder.Record = false;
		}
	}

	public virtual void SetFreezing(bool freezing)
	{
		p.freezing = freezing;
		if(freezing) body.constraints = RigidbodyConstraints.FreezeAll;
	}

	public void GetErrorVelocity(Vector3 error, Vector3 speed, out float out_error, out float out_speed)
	{
		out_error = error.magnitude;
		if (out_error > 0) error /= out_error;
		out_speed = Vector3.Dot(error, speed);
	}

	public void RecordStat(string name, float value, StatAggregationMethod method = StatAggregationMethod.Average)
	{
		if (statsRecorder != null)
		{
			statsRecorder.Add(name, value, method);
		}
	}

	public static Vector3 Horizontal(Vector3 vec)
	{
		vec.y = 0;
		return vec;
	}

	public static Vector2 Horizontal2(Vector3 vec)
	{
		vec.y = vec.z;
		return vec;
	}

}
