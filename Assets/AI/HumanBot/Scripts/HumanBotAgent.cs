using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

using static Unity.MLAgents.StatAggregationMethod;

public class HumanBotAgent : JointAgent
{

	[System.NonSerialized]
	public new HumanBotParameters p;

    protected DuelMaster duelEnv;

	protected float heightFactor;

	protected bool dying = false;
    protected int dyingFrames = 0;

    protected float deathReward = 0;
    protected float winReward = 0;
	protected bool won = false;
	protected bool dead = false;

    protected Referential rootReferential;

	private float speed;

    private int arcFrameCounter = 0;

	private Vector3 rootPosition;
	private Vector3 targetPosition;
	private float targetSpeed;
	private Vector3 targetSpeedVector;

	private VectorSensorComponent nogailSensorComponent;

	public GameEvent resetChunksEvent;

	public override void Initialize()
    {
		p = GetComponent<HumanBotParameters>();

		rootReferential = new UnorthogonalReferential(p.root.transform);
		referential = rootReferential;

		base.Initialize();

		heightFactor = p.height / EstimateHeight();

		if (p.environment is DuelMaster)
        {
            duelEnv = (DuelMaster)p.environment;
        }

		foreach(var foot in p.feet) {
			foot.collisionPenalty = 0;
			foreach (var toes in foot.GetComponentsInChildren<BotJoint>())
            {
                toes.collisionPenalty = 0;
            }
		}

		speed = p.targetSpeed;

		UpdateObjective();

		nogailSensorComponent = GetComponent<VectorSensorComponent>();

	}

    public override void OnEpisodeBegin()
    {
		resetChunksEvent.Raise();
        RandomSpeed();

		if(pullJoint != null) Destroy(pullJoint);

        arcFrameCounter = 0;
		dyingFrames = 0;

		base.OnEpisodeBegin();

		UpdateObjective();
	}

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        arcFrameCounter++;

		ArcUpdate();

        KillCondition();

        if (Randomizing() && p.pushing) Push();
		if (Randomizing() && p.pulling) Pull();

	}

	public override void OnEpisodeEnd()
	{
		base.OnEpisodeEnd();

        WinCondition();

		RecordStat("Win/Win Reward", winReward);
		RecordStat("Death/Death Reward", deathReward);
		RecordStat("Win/Won", won ? 1f : 0f);
		RecordStat("Death/Dead", dead ? 1f : 0f);

		won = false;
		dead = false;
		winReward = 0f;
		deathReward = 0f;

	}

	protected Normalizer headSpringNorm = new Normalizer(2f);
	protected Normalizer headDamperNorm = new Normalizer(1f);
	protected Normalizer rootSpringNorm = new Normalizer(10f);

	protected float maxHeight = 0, minHeight = 10, maxRoot = 0;
	protected Normalizer rootDamperNorm = new Normalizer(1f);

	public virtual void CalculateRewards()
    {
        if (!p.training) return;

		// No rewards if first decision
		if (decisionCounter == 0) return;

        // height error
        float headError = GetHeadError();
        float headSpeed = Vector3.Dot(Vector3.up, p.head.body.velocity);
		float headDamper = headDamperNorm.Normalize(headSpeed) * p.headDamperReward;
        float headSpring = -headSpringNorm.Normalize(headError) * p.headSpringReward;

        RecordStat("Head/Head Spring Error", headError, Histogram);
        RecordStat("Head/Head Damper Speed", headSpeed, Histogram);
		RecordStat("Head/Head Max Speed", headDamperNorm.max);
		RecordStat("Head/Head Max Error", headSpringNorm.max);
		maxHeight = Mathf.Max(maxHeight, p.head.transform.position.y);
		minHeight = Mathf.Min(minHeight, p.head.transform.position.y);
		RecordStat("Head/Head Max Height", maxHeight);
		RecordStat("Head/Head Min Height", minHeight);
		RecordStat("Head/Head Height", GetCurrentHeight());
		RecordStat("Head/Head Spring Reward", headSpring, Histogram);
		RecordStat("Head/Head Damper Reward", headDamper, Histogram);

		dying = IsDying();

        float dyingReward = dying ? p.dyingReward : p.lifeReward;
		RecordStat("Death/Dying Reward", dyingReward);
		RecordStat("Death/Alive", dying ? 0f : 1f);

		RecordStat("Death/Alive " + targetSpeed + "m.s", dying ? 0f : 1f);

		// horizontal error
		var rootVelocity = (p.root.transform.position - rootPosition) / (Time.fixedDeltaTime * p.decisionPeriod);

        var rootSpeed = Vector3.Dot(rootVelocity, targetSpeedVector.normalized);
        var rootError = Horizontal(targetSpeedVector - rootVelocity).magnitude;
        
        //float rootDamper = -rootDamperNorm.Normalize(Mathf.Min(rootSpeed, p.targetSpeed)) * p.rootReward;
        float rootSpring = -rootSpringNorm.Normalize(rootError) * p.rootReward;
        //if(p.targetSpeed == 0f) rootDamper = -rootDamperNorm.Normalize(p.root.body.velocity.magnitude) * p.rootReward;

		//float rootDamper = Mathf.Exp(-rootError*rootError) * p.rootReward;
        float rootDamper = Mathf.Min(rootSpeed, targetSpeed) * p.rootReward;
        if(targetSpeed > 0) rootDamper /= targetSpeed;

		float derivative = -2f * rootError * rootDamper;

		RecordStat("Root/Root Max Speed", rootDamperNorm.max);
		RecordStat("Root/Root Max Error", rootSpringNorm.max);
		maxRoot = Mathf.Max(maxRoot, Horizontal(p.root.transform.position).magnitude);
		RecordStat("Root/Root Max Distance", maxRoot);
		RecordStat("Root/Root Spring Error", rootError, Histogram);
		RecordStat("Root/Root Spring Reward", rootSpring, Histogram);

		var horizontalVelocity = Horizontal(rootVelocity).magnitude;
		var totalVelocity = rootVelocity.magnitude;

		RecordStat("Root/Root Speed", rootSpeed, Histogram);
		RecordStat("Root/Root Speed Error", rootError, Histogram);
		RecordStat("Root/Root Damper Reward", rootDamper, Histogram);
		RecordStat("Root/Root Damper Reward Derivative", derivative, Histogram);
        RecordStat("Root/Root Total Velocity", totalVelocity, Histogram);
        RecordStat("Root/Root Horizontal Velocity", horizontalVelocity, Histogram);

		RecordStat("Root/Root Speed " + targetSpeed + "m.s", rootSpeed, Histogram);
		RecordStat("Root/Root Speed Error " + targetSpeed + "m.s", rootError, Histogram);
		RecordStat("Root/Root Damper Reward " + targetSpeed + "m.s", rootDamper, Histogram);
		RecordStat("Root/Root Damper Reward Derivative " + targetSpeed + "m.s", derivative, Histogram);
		RecordStat("Root/Root Total Velocity " + targetSpeed + "m.s", totalVelocity, Histogram);
        RecordStat("Root/Root Horizontal Velocity " + targetSpeed + "m.s", horizontalVelocity, Histogram);

		RecordStat("Root/Root Target Speed", targetSpeed, Histogram);

		// Energy Reward (per frame)

		float frameConsumedEnergy = consumedEnergy / (Time.fixedDeltaTime * p.decisionPeriod);
		consumedEnergy = 0;

		float energyPenalty = frameConsumedEnergy;
		//if (targetSpeed > 0) energyPenalty /= Mathf.Clamp(rootSpeed, 0.01f, targetSpeed);
		//else energyPenalty *= 60f/4.5f;

		var norm = 0.125f - energyPenalty / 10000000f;

		float energyReward = norm * p.energyPenalty;

		RecordStat("Energy/Frame Consumed Energy", frameConsumedEnergy, Histogram);
		RecordStat("Energy/Energy Penalty", energyPenalty, Histogram);
		RecordStat("Energy/Frame Energy Reward", energyReward, Histogram);
		
		RecordStat("Energy/Frame Consumed Energy " + targetSpeed + "m.s", frameConsumedEnergy, Histogram);
		RecordStat("Energy/Energy Penalty " + targetSpeed + "m.s", energyPenalty, Histogram);
		RecordStat("Energy/Frame Energy Reward " + targetSpeed + "m.s", energyReward, Histogram);

		// Add rewards
		double reward = 0;

        // If dying
        reward += dyingReward;

        if(dying)
        {
			// Get head back up
			reward += headSpring;
			reward += headDamper;
		} else {
			// Get root to target
			//reward += rootSpring;
			reward += rootDamper;

			// Consume less energy
			reward += energyReward;
		}

		// scale reward with period of decisions/rewards
		//reward *= p.decisionPeriod;

		RecordStat("Environment/Total Frame Reward", (float)reward, Histogram);
		if(dying)
		{
			RecordStat("Environment/Total Dying Frame Reward", (float)reward, Histogram);
		} else
		{
			RecordStat("Environment/Total Alive Frame Reward", (float)reward, Histogram);
		}

		AddReward(Mathf.Max(0, (float) reward));
    }

	public virtual void CollectImitatedObservations(VectorSensor sensor)
	{
		Vector3 targetSpeed = targetSpeedVector;
		if(dying) targetSpeed = Vector3.zero;

		Vector3 speed = Horizontal(p.root.body.velocity);


		var diff = targetSpeed - speed;

		if (p.speedDiffZeroRecording && p.recording)
			diff = Vector3.zero;

		diff.y = GetHeadError();

		if (p.speedDiffZero)
			diff = Vector3.zero;

		sensor.AddObservation(referential.InverseTransformVector(diff));

		if(p.targetSpeedObs) {
			var vel = p.targetSpeed;
			if(this.speed != 0) vel /= this.speed;
			sensor.AddObservation(vel);
		}
		if(p.dyingObs) {
			sensor.AddObservation(dying);
		}

	}

	public virtual void CollectGoalObservations(VectorSensor sensor)
	{
		var v = referential.InverseTransformVector(targetSpeedVector);
		Vector2 v2 = new Vector2(v.x, v.z);
		sensor.AddObservation(v2);
	}

	public override void CollectObservations(VectorSensor sensor)
    {

        CalculateRewards();
        
        UpdateObjective();

		if(nogailSensorComponent)
		{
			var nogailSensor = nogailSensorComponent.GetSensor();
			if(nogailSensor != null) CollectGoalObservations(nogailSensor);
		}

		CollectImitatedObservations(sensor);
		
        base.CollectObservations(sensor);

	}

	public void UpdateObjective()
	{
		if (!p.target) return;
		rootPosition = p.root.transform.position;
        targetPosition = p.target.position;
        targetSpeed = p.targetSpeed;
		targetSpeedVector = Horizontal(p.target.position - p.root.transform.position).normalized * targetSpeed;
		referential.rootVelocity = p.relativeVelocity ? p.root.realVelocity : Vector3.zero;
	}

    public override void SetTarget()
    {
        if (p.targetMover)
        {
            Vector3 targetPosition = p.head.transform.position;
            if(Randomizing() && p.randomTarget) {
				p.targetMover.randomRadius(targetPosition, 20, 30);
            } else
            {
                p.target.position = targetPosition + Vector3.forward * 1000f;
            }
        }
	}

    public void RandomSpeed()
    {
		if (Randomizing() && p.randomSpeed)
		{
			float rand = Random.value;
			float i = 0;
			foreach(float speed in p.speeds)
			{
				i++;
				if(rand <= i/p.speeds.Length)
				{
					p.targetSpeed = speed;
					break;
				}
			}
		}
	}

	public Rigidbody GetRandomBody() {
		int i = Random.Range(0, joints.Count + 1);
		Rigidbody rb = i == joints.Count ? body : joints[i].body;
		return rb;
	}

	public override void Push()
	{
		if(Random.value <= p.pushChance) {
			var rb = GetRandomBody();
			float strength = p.pushForce * p.totalMass;
			Vector3 force = Random.insideUnitSphere * strength;
			force.y = Mathf.Abs(force.y);
			Vector3 torque = Random.insideUnitSphere * strength;
			rb.AddForce(force, ForceMode.Impulse);
			rb.AddTorque(torque, ForceMode.Impulse);
			Debug.DrawRay(rb.position, force, Color.red, 1f);
		}
	}

    ConfigurableJoint pullJoint;
    float pullTimer = -1f;
	Vector3 pullPosition;

    public void Pull()
    {
        if (pullJoint == null && Random.value <= p.pullChance)
        {
			var rb = GetRandomBody();
			pullJoint = rb.gameObject.AddComponent<ConfigurableJoint>();

			pullJoint.xMotion = ConfigurableJointMotion.Free;
			pullJoint.yMotion = ConfigurableJointMotion.Free;
			pullJoint.zMotion = ConfigurableJointMotion.Free;
			pullJoint.angularXMotion = ConfigurableJointMotion.Free;
			pullJoint.angularYMotion = ConfigurableJointMotion.Free;
			pullJoint.angularZMotion = ConfigurableJointMotion.Free;

			var drive = new JointDrive();
			drive.maximumForce = 10000f * rb.mass * p.pullForce;
			drive.positionSpring = 100f * rb.mass * p.pullForce;
			drive.positionDamper = 10f * rb.mass * p.pullForce;
			
			pullJoint.xDrive = drive;
			pullJoint.yDrive = drive;
			pullJoint.zDrive = drive;
			pullJoint.slerpDrive = drive;

			pullJoint.rotationDriveMode = RotationDriveMode.Slerp;

			pullJoint.targetRotation = Random.rotationUniform;
			pullJoint.targetPosition = Random.insideUnitSphere * 0.2f;
			pullPosition = rb.transform.position - pullJoint.targetPosition;

            pullTimer = Time.time + Random.value*2;
		}

		if(pullJoint != null) {
			Debug.DrawLine(pullJoint.transform.position, pullPosition, Color.red, 0f);
		}

        if(pullJoint != null && pullTimer < Time.time)
        {
			Destroy(pullJoint);
		}

    }

	public virtual void ArcUpdate()
	{
		if (p.training && arcFrameCounter > p.arcFrames)
		{
			arcFrameCounter = 0;

			SetTarget();
			RandomSpeed();
		}
	}

	public virtual void WinCondition()
    {
		if(!dead)
		{
			Win();
		}
	}

	public void Win()
	{
		float reward = WinRewards();

		AddReward(reward);
        
        won = true;
	}

	public virtual float WinRewards()
    {
        if (!p.training) return 0f;

		float reward = 0f;

		float winR = p.winReward;
		winReward += winR;
		reward += winR;

		
		float energyPerFrame = totalConsumedEnergy / frameCounter;
        float energyReward = -energyPerFrame * p.energyPenalty;
		RecordStat("Energy/Total Consumed Energy", totalConsumedEnergy, Histogram);
		RecordStat("Energy/Frame Consumed Energy", energyPerFrame, Histogram);
		RecordStat("Energy/Energy Reward", energyReward, Histogram);
		//reward += energyReward;

		totalConsumedEnergy = 0;
		

		RecordStat("Win/Total Win Reward", reward, Histogram);

		return reward;
	}


	public void KillCondition()
    {
		// if not standing up high enough
		dying = IsDying();

		if (dying && p.dieOnFall)
		{
			dyingFrames++;
			if (dyingFrames > p.maxDyingFrames)
			{
				Kill();
			}
		}
		else
		{
			if(p.resetOnAlive) dyingFrames = 0;
		}
	}

	public void Kill()
	{
		float reward = DeathRewards();

		AddReward(reward);

		dead = true;

		OnEpisodeEnd();
		EndEpisode();

		//if (!p.training) Destroy(gameObject);
	}

	public float DeathRewards()
    {
		deathReward = p.deathPenalty;

		//float lifeReward = frameCounter;
		RecordStat("Death/Death Frames", frameCounter, Histogram);
		//RecordStat("Death/Life Reward", lifeReward);

		float reward = deathReward;

		RecordStat("Death/Total Death Reward", reward);

		return reward;
	}

    private float GetHeadError()
    {
        CheckJoint(ref p.head);
		var headError = (p.height - GetCurrentHeight())/p.height;
        return Mathf.Clamp01(headError);
    }

	private float EstimateHeight()
	{
		// todo fix
		var rightDistance = Vector3.Dot(p.head.transform.position - p.feet[0].transform.position, Vector3.up);
		var leftDistance = Vector3.Dot(p.head.transform.position - p.feet[1].transform.position, Vector3.up);

		var height = Mathf.Max(rightDistance, leftDistance, 0f);

		return height;
	}

    public float GetCurrentHeight()
    {
		return heightFactor*EstimateHeight();
    }

    public bool IsDying()
    {
        return GetCurrentHeight() < p.dyingHeightFactor * p.height;
    }

	public override void UpdateParameters()
	{
		base.UpdateParameters();

		p = GetComponent<HumanBotParameters>();

		if (p.training)
		{
			UpdateParameter("rootReward", ref p.rootReward);
			UpdateParameter("headSpringReward", ref p.headSpringReward);
			UpdateParameter("headDamperReward", ref p.headDamperReward);
			UpdateParameter("winReward", ref p.winReward);
			UpdateParameter("deathPenalty", ref p.deathPenalty);
			UpdateParameter("dyingHeightFactor", ref p.dyingHeightFactor);
			p.maxDyingFrames = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("maxDyingFrames", p.maxDyingFrames);
			UpdateParameter("lifeReward", ref p.lifeReward);
			UpdateParameter("dyingReward", ref p.dyingReward);
		}
	}

}