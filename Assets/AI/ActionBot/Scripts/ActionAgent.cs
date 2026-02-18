using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Demonstrations;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;

using static Unity.MLAgents.StatAggregationMethod;

/*
 * High level agent that decides global movement to go given environment information
 * Controls the head movement
 * Will be extended to control head rotation, then hand position/rotation
 * Is built for imitation learning from VR recorded data
 * Can easily be combined with HumanBotAgent to autonomously choose the path that it will take
 * 
 * TODO:
 * Implement head rotation
 * Move recording out of this script
 * Merge BotAgent with the shared parts of this script (there are many)
 * Add acceleration so it can't attain full speed immediately
 * Add speed cap
 * Add hand movement
 * 
 */
public class ActionAgent : Agent
{

	[Header("Members")]
	public Transform head;

	// Hands are not yet implemented
	public Transform leftHand, rightHand;
	public Grabber leftHandGrabber, rightHandGrabber;

	[Header("Training")]
	public int maxFrames = 0;
	// Changes maxFrames every episode by [-variance;variance]
	public int maxFramesVariance = 0;
	[System.NonSerialized]
	public int maxMaxFrames = 0;
	public bool training = false;

	[Header("Behavior")]
	public bool recording = false;
	public int decisionPeriod = 1;

	[Header("Reward")]
	public float rewardFactor = 1f;

	[Header("Movement")]
	public float headMaxSpeed = 5f;
	public bool useAcceleration = false;
	public float acceleration = 1000f;
	public float handReach = 1.20f;
	public float fullRotation = 360f;

	[System.NonSerialized]
	public StatsRecorder statsRecorder;

	[System.NonSerialized]
	public Referential referential;

	private Transform referentialTransform;

	[System.NonSerialized]
	public int frameCounter = 0;

	protected bool ending = false;

	private Speed headVelocity;
	private Speed[] handVelocities;

	private Transform[] hands;

	private Grabber[] grabbers;

	private BehaviorParameters behavior;

	private Replayer replayer;

	private Restarter[] restarters;

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

		Academy.Instance.AgentPreStep += RequestDecisions;

		statsRecorder = training ? Academy.Instance.StatsRecorder : null;
		if (!training) maxFrames = 0;

		var go = new GameObject("Referential");
		referentialTransform = go.transform;
		referentialTransform.parent = transform;

		referential = new Referential(referentialTransform);
		UpdateReferential();

		if (!leftHandGrabber && leftHand) leftHandGrabber = leftHand.GetComponent<Grabber>();
		if (!rightHandGrabber && rightHand) rightHandGrabber = rightHand.GetComponent<Grabber>();

		headVelocity = head.GetComponent<Speed>();
		handVelocities = new Speed[] { leftHand ? leftHand.GetComponent<Speed>() : null, rightHand ? rightHand.GetComponent<Speed>() : null};
		hands = new Transform[] { leftHand, rightHand };
		grabbers = new Grabber[] { leftHandGrabber, rightHandGrabber };

		replayer = GetComponent<Replayer>();
		behavior = GetComponent<BehaviorParameters>();

		restarters = transform.GetComponentsInChildren<Restarter>();

		maxMaxFrames = maxFrames;

		SetRecording(recording);

		events = GetComponent<AgentEvents>();
	}

    public override void OnEpisodeBegin()
	{
		frameCounter = 0;
		ending = false;

		Restart();

		SetRecording(recording);

		// Vary maxframes at every episode
		if(!recording) maxFrames = maxMaxFrames - UnityEngine.Random.Range(-maxFramesVariance, maxFramesVariance);

		events.SignalBeginEpisode();
	}

	protected void FixedUpdate()
	{
		frameCounter++;

		// Check if record replaying has finished
		if (recording && replayer.IsLoaded() && !replayer.IsPlaying())
		{
			ending = true;
		}

		if (ending || (maxFrames > 0 && frameCounter > maxFrames))
		{
			ending = false;
			OnEpisodeEnd();
			EpisodeInterrupted();
		}

	}

	public void calculateRewards()
	{
		if (!training) return;

		// Specific scenario rewards would be outside of this script

	}

	public override void CollectObservations(VectorSensor sensor)
	{
		
		UpdateReferential();

		events.SignalCollectObservations(sensor);

		calculateRewards();

		// Specific scenario observations would be outside of this script

		// TODO: Add observations to help controlling the head rotation (how ?)

		sensor.AddObservation(referential.InverseTransformVector(headVelocity.speedVector));
		
		sensor.AddObservation(referential.InverseTransformVector(Vector3.forward));
		sensor.AddObservation(referential.InverseTransformVector(Vector3.up));
		
		//sensor.AddObservation(transform.TransformVector(head.right));
		foreach (Transform hand in hands)
		{
			//Vector3 primary = transform.TransformVector(hand.forward);
			//Vector3 secondary = transform.TransformVector(hand.right);
			//sensor.AddObservation(referential.InverseTransformVector(primary));
			//sensor.AddObservation(referential.InverseTransformVector(secondary));
			//sensor.AddObservation(referential.InverseTransformPoint(hand.position));
		}

		foreach (Grabber grabber in grabbers)
		{
			//sensor.AddObservation(grabber ? (grabber.grabbed ? 1 : 0) : 0);
		}

	}

	public override void OnActionReceived(ActionBuffers actions)
	{
		UpdateReferential();

		int i = 0;

		events.SignalActionReceived(actions, ref i);

		//int discreteIndex = 0;

		// Action for moving the head
		var worldHeadMovement = GetMoveAction(ref i, actions, IntegrateSpeed(headMaxSpeed), false);
		RecordStat("Action Bot/Head Speed", DerivateSpeed(worldHeadMovement.magnitude), Histogram);
		head.position += worldHeadMovement;
		
		
		// TODO: Uncomment, test and, if not working, fix this head rotation code

		var headRotation = GetRotateAction(ref i, actions);
		RecordStat("Action Bot/Head Rotation Speed", DerivateSpeed(Quaternion.Angle(head.rotation, headRotation)), Histogram);
		head.rotation = headRotation;

		foreach (Transform hand in hands)
		{
			//var worldHandMovement = GetMoveAction(ref i, actions, handReach);
			//hand.position = head.position + worldHandMovement;
			//var handRotation = GetRotateAction(ref i, actions, fullRotation);
			//hand.rotation = head.rotation * handRotation;
		}

		foreach (Grabber grabber in grabbers)
		{
			//int grabAction = actions.DiscreteActions[discreteIndex++];
			//if (grabber) grabber.SetGrabbing(grabAction == 1 ? true : false);
		}

		//Debug.Log(i);
		
		UpdateReferential();
	}

	public override void Heuristic(in ActionBuffers actionsOut)
	{
		UpdateReferential();

		actionsOut.Clear();

		int i = 0;
		//int discreteIndex = 0;

		var continuousActions = actionsOut.ContinuousActions;
		//var discreteActions = actionsOut.DiscreteActions;

		var headDiff = replayer.player.position - head.position;
		SetMoveAction(ref i, ref continuousActions, IntegrateSpeed(headMaxSpeed), headDiff, false);

		var headRotation = replayer.player.rotation;
		SetRotateAction(ref i, ref continuousActions, headRotation);

		//var leftHandDiff = replayer.leftHand.position - replayer.player.position;
		//SetMoveAction(ref i, ref continuousActions, handReach, leftHandDiff);

		//var leftHandRotation = Quaternion.Inverse(replayer.player.rotation) * replayer.leftHand.rotation;
		//SetRotateAction(ref i, ref continuousActions, fullRotation, leftHandRotation);

		//var rightHandDiff = replayer.rightHand.position - replayer.player.position;
		//SetMoveAction(ref i, ref continuousActions, handReach, rightHandDiff);

		//var rightHandRotation = Quaternion.Inverse(replayer.player.rotation) * replayer.rightHand.rotation;
		//SetRotateAction(ref i, ref continuousActions, fullRotation, rightHandRotation);

		//var leftGrabber = replayer.leftHand.GetComponent<Grabber>();
		//int leftGrabAction = leftGrabber.grabbing ? 1 : 0;
		//discreteActions[discreteIndex++] = leftGrabAction;

		//var rightGrabber = replayer.rightHand.GetComponent<Grabber>();
		//int rightGrabAction = rightGrabber.grabbing ? 1 : 0;
		//discreteActions[discreteIndex++] = rightGrabAction;

	}

	public Vector3 GetMoveAction(ref int i, ActionBuffers actions, float magnitude, bool vertical = true)
	{
		// this is a cube of action
		Vector3 action = new Vector3(actions.ContinuousActions[i++], vertical ? actions.ContinuousActions[i++] : 0, actions.ContinuousActions[i++]);
		var localMovement = action * magnitude;

		var worldMovement = referential.TransformVector(localMovement);
		if(!vertical) worldMovement.y = 0;
		return worldMovement;
	}

	public void SetMoveAction(ref int i, ref ActionSegment<float> continuousActions, float magnitude, Vector3 diff, bool vertical = true)
	{
		var worldMovement = diff;
		if (!vertical) worldMovement.y = 0;
		var localMovement = referential.InverseTransformVector(worldMovement);
		CheckMagnitude(localMovement, magnitude);
		var action = localMovement / magnitude;
		continuousActions[i++] = action.x;
		if(vertical) continuousActions[i++] = action.y;
		continuousActions[i++] = action.z;
	}

	// Extract local forward vector and transform into world space vector with referential
	// then transform into world space rotation
	public Quaternion GetRotateAction(ref int i, ActionBuffers actions, bool vertical = true)
	{

		Vector3 action = new Vector3(actions.ContinuousActions[i++], vertical ? actions.ContinuousActions[i++] : 0, actions.ContinuousActions[i++]);
		var localForward = action;

		var worldForward = referential.TransformVector(localForward);
		if(!vertical) worldForward.y = 0;

		var worldRotation = Quaternion.LookRotation(worldForward, Vector3.up);

		return worldRotation;
	}

	public void SetRotateAction(ref int i, ref ActionSegment<float> continuousActions, Quaternion worldRotation, bool vertical = true)
	{
		var worldForward = worldRotation * Vector3.forward;
		if (!vertical) worldForward.y = 0;
		var localForward = referential.InverseTransformVector(worldForward);
		var action = localForward;

		continuousActions[i++] = action.x;
		if(vertical) continuousActions[i++] = action.y;
		continuousActions[i++] = action.z;
	}

	public float IntegrateSpeed(float speed)
	{
		return speed * Time.fixedDeltaTime * decisionPeriod;
	}

	public float DerivateSpeed(float movement)
	{
		return movement / (Time.fixedDeltaTime * decisionPeriod);
	}

	public void CheckMagnitude(Vector3 vector, float magnitude)
	{
		if (Mathf.Abs(vector.x) > magnitude ||
			Mathf.Abs(vector.y) > magnitude ||
			Mathf.Abs(vector.z) > magnitude)
		{
			Debug.Log("vector > magnitude : " + vector + " > " + magnitude);
		}
	}


	// Utility functions
	
	// Updates Reference frame defining a local space for observations and actions to coexist in.
	// Currently has a fixed rotation
	// Needs to be called before it is used.
	// Could be done automatically in a separate Referential subclass
	private void UpdateReferential()
    {
		referentialTransform.position = head.position;
		referentialTransform.rotation = Quaternion.LookRotation(head.forward, Vector3.up);
    }

	public void OnEpisodeEnd()
	{
		events.SignalEndEpisode();

		Events.SignalDestroy(gameObject);
	}

	public void RequestDecisions(int academyStepCount)
	{
		if (academyStepCount % decisionPeriod == 0)
		{
			RequestDecision();
		}
	}
	
	public void Restart()
	{
		// replace all at beginning
		foreach(Restarter restarter in restarters)
		{
			restarter.Restart();
		}

		headVelocity.ResetPosition();
		foreach (Speed handVelocity in handVelocities) handVelocity?.ResetPosition();
	}

	public new void AddReward(float reward)
	{
		if (training)
		{
			base.AddReward(reward * rewardFactor);
		}
	}

	// TODO: Move recording outside of this script to allow for non imitation based action agents
	public void SetRecording(bool recording)
	{
		this.recording = recording;
		DemonstrationRecorder recorder = GetComponent<DemonstrationRecorder>();
		if (recording)
		{
			recorder.Record = true;
			replayer.Play();
			behavior.BehaviorType = BehaviorType.HeuristicOnly;
			MoveToGhost();
		}
		else if (recorder)
		{
			recorder.Record = false;
		}
	}

	// Move agent to current replayer (recorded) state
	public void MoveToGhost()
	{
		CopyTransform(replayer.player, head);
		CopyTransform(replayer.leftHand, leftHand);
		CopyTransform(replayer.rightHand, rightHand);
	}

	public void CopyTransform(Transform source, Transform target)
	{
		target.position = source.position;
		target.rotation = source.rotation;
	}

	public void RecordStat(string name, float value, StatAggregationMethod method = StatAggregationMethod.Average)
	{
		if (statsRecorder != null)
		{
			statsRecorder.Add(name, value, method);
		}
	}

	public void SetSpawn(Transform head, Transform leftHand, Transform rightHand)
	{
		this.head.position = head.position;
		this.head.rotation = head.rotation;
		this.leftHand.position = leftHand.position;
		this.leftHand.rotation = leftHand.rotation;
		this.rightHand.position = rightHand.position;
		this.rightHand.rotation = rightHand.rotation;
	}

}
