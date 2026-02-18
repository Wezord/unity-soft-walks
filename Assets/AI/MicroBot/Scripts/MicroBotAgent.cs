using Unity.MLAgents.Sensors;
using UnityEngine;

using static Unity.MLAgents.StatAggregationMethod;

public class MicroBotAgent : JointAgent
{

	[System.NonSerialized]
	public new MicroBotParameters p;

    protected Vector3 headStart;

    protected float height;

    protected bool dying = false;
    protected int dyingFrames = 0;

    protected float deathReward = 0;
    protected float winReward = 0;

    protected Referential rootReferential;

    private int arcFrameCounter = 0;

	public override void Initialize()
    {
        base.Initialize();

        p = GetComponent<MicroBotParameters>();

		rootReferential = new UnorthogonalReferential(p.root.transform);
        referential = rootReferential;

		headStart = p.head.transform.position;
        height = getCurrentHeight();

	}

    public override void OnEpisodeBegin()
    {
        if (frameCounter > 0)
        {
            RecordStat("Win/Win Reward", winReward, Histogram);
            RecordStat("Death/Death Reward", deathReward, Histogram);
        }
        winReward = 0f;
        deathReward = 0f;

		if(pullJoint != null) Destroy(pullJoint);
		
        arcFrameCounter = 0;
		dyingFrames = 0;

		base.OnEpisodeBegin();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        arcFrameCounter++;

        KillCondition();

        if (Randomizing() && p.pushing && Random.value <= 0.001f) Push();
		if (Randomizing() && p.pulling) Pull();

	}

	protected Normalizer headSpringNorm = new Normalizer(2f);
	protected Normalizer headDamperNorm = new Normalizer(1f);
	protected Normalizer rootSpringNorm = new Normalizer(10f);
	protected Normalizer rootDamperNorm = new Normalizer(1f);

	public virtual void calculateRewards()
    {
        if (!p.training) return;

        // height error
        float headError = GetHeadError();
        float headSpeed = Vector3.Dot(Vector3.up, p.head.velocity);
		float headDamper = headDamperNorm.Normalize(headSpeed) * p.headReward;
        float headSpring = -headSpringNorm.Normalize(headError) * p.headReward;
        RecordStat("Head/Head Spring Error", headError, Histogram);
        RecordStat("Head/Head Damper Speed", headSpeed, Histogram);
		RecordStat("Head/Head Max Speed", headDamperNorm.max);
		RecordStat("Head/Head Max Error", headSpringNorm.max);
		RecordStat("Head/Head Spring Reward", headSpring, Histogram);
		RecordStat("Head/Head Damper Reward", headDamper, Histogram);

		dying = IsDying();

        float dyingReward = dying ? -3f : 1f;
		RecordStat("Death/Dying Reward", dyingReward, Histogram);
		RecordStat("Death/Alive", dying ? 0f : 1f, Histogram);

		// horizontal error
		GetErrorVelocity(GetRootError(), p.root.velocity, out float rootError, out float rootSpeed);
		float rootDamper = rootDamperNorm.Normalize(rootSpeed) * p.rootReward;
		float rootSpring = -rootSpringNorm.Normalize(rootError) * p.rootReward;

		RecordStat("Root/Root Max Speed", rootDamperNorm.max);
		RecordStat("Root/Root Max Error", rootSpringNorm.max);
		RecordStat("Root/Root Spring Error", rootError, Histogram);
		RecordStat("Root/Root Damper Speed", rootSpeed, Histogram);
		RecordStat("Root/Root Spring Reward", rootSpring, Histogram);
		RecordStat("Root/Root Damper Reward", rootDamper, Histogram);

		// Add rewards
		double reward = 0;

        // If dying
        //reward += dyingReward;

		/*
        if(dying)
        {
			// Get head back up
			reward += headSpring;
			reward += headDamper;
		}
		*/

        // Get root to target
        //reward += rootSpring;
        reward += rootDamper;

		// scale reward with period of decisions/rewards
		reward *= p.decisionPeriod;

		RecordStat("Environment/Total Frame Reward", (float)reward, Histogram);

		AddReward((float) reward);
    }

    public override void CollectObservations(VectorSensor sensor)
    {

        calculateRewards();

        base.CollectObservations(sensor);

    }

	public override void SetTarget()
	{
		if (p.targetMover)
		{
			Vector3 targetPosition = p.head.transform.position;
			targetPosition.y = headStart.y;
			if (Randomizing() && p.randomTarget)
			{
				p.targetMover.randomRadius(targetPosition, 20, 30);
			}
			else
			{
				p.target.position = targetPosition + Vector3.forward * 1000;
			}
		}
	}

	public override void Push()
	{
		float strength = 5f * p.totalMass;
		Vector3 force = Random.insideUnitSphere * strength;
		Vector3 torque = Random.insideUnitSphere * strength;
		Vector3 position = p.root.transform.position;
		position.y = Random.Range(p.head.transform.position.y - getCurrentHeight(), p.head.transform.position.y);
		p.root.AddForceAtPosition(force, position, ForceMode.Impulse);
		p.root.AddTorque(torque, ForceMode.Impulse);
	}

    ConfigurableJoint pullJoint;
    float pullTimer = -1f;

    public void Pull()
    {
        if (pullJoint == null && Random.value <= 0.001f)
        {
			int i = Random.Range(0, joints.Count + 1);
			Rigidbody rb = i == joints.Count ? body : joints[i].body;
			pullJoint = rb.gameObject.AddComponent<ConfigurableJoint>();

			pullJoint.xMotion = ConfigurableJointMotion.Free;
			pullJoint.yMotion = ConfigurableJointMotion.Free;
			pullJoint.zMotion = ConfigurableJointMotion.Free;
			pullJoint.angularXMotion = ConfigurableJointMotion.Free;
			pullJoint.angularYMotion = ConfigurableJointMotion.Free;
			pullJoint.angularZMotion = ConfigurableJointMotion.Free;

			var drive = new JointDrive();
			drive.maximumForce = 10000f * rb.mass;
			drive.positionSpring = 100f * rb.mass;
			drive.positionDamper = 10f * rb.mass;
			
			pullJoint.xDrive = drive;
			pullJoint.yDrive = drive;
			pullJoint.zDrive = drive;
			pullJoint.slerpDrive = drive;

			pullJoint.rotationDriveMode = RotationDriveMode.Slerp;

			pullJoint.targetRotation = Random.rotationUniform;
			pullJoint.targetPosition = Random.insideUnitSphere * 0.2f;

            pullTimer = Time.time + Random.value*2;
		}

        if(pullJoint != null && pullTimer < Time.time)
        {
			Destroy(pullJoint);
		}

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
			dyingFrames = 0;
		}
	}

	public void Kill()
	{
		float reward = DeathRewards();

		AddReward(reward);
		OnEpisodeEnd();
		EndEpisode();

		//if (!p.training) Destroy(gameObject);
	}

	public float DeathRewards()
    {
		deathReward = -1000;

		//float lifeReward = frameCounter;
		RecordStat("Death/Death Frames", frameCounter, Histogram);
		//RecordStat("Death/Life Reward", lifeReward);

		float reward = deathReward;

		RecordStat("Death/Total Death Reward", reward, Histogram);

		return reward;
	}

	

    private float GetHeadError()
    {
        Vector3 headErrorVector = headStart - p.head.transform.position;
        float headError = Mathf.Abs(headErrorVector.y);
        return headError;
    }

    public Vector3 GetRootError()
    {
        Vector3 rootErrorVector = p.target.position - p.root.transform.position;
        rootErrorVector.y = 0;
        return rootErrorVector;
    }

    public void GetRootErrorVelocity(out float out_error, out float out_speed)
    {
        GetErrorVelocity(GetRootError(), p.root.velocity, out out_error, out out_speed);
    }
    

    public float getCurrentHeight()
    {

		float maxDistance = 0f;
		foreach (var foot in p.feet)
		{
			maxDistance = Mathf.Max(maxDistance, Vector3.Dot(p.head.transform.position - foot.transform.position, Vector3.up));
		}

        return maxDistance;
    }

    public bool IsDying()
    {
        return getCurrentHeight() < p.dyingHeightFactor * height;
    }

}