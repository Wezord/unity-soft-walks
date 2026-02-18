using Unity.MLAgents.Sensors;
using UnityEngine;

using static Unity.MLAgents.StatAggregationMethod;

public class SoccerBotAgent : HumanBotAgent
{
    public Transform ball;
	public Transform target;

	public float ballRadius = 0.5f;

	public float ballSpringReward = 1f;
	public float ballDamperReward = 1f;
	public float ballVelocityReward = 1f;

	public float ballWinDistance = 1f;
	public float ballWinReward = 1f;
	private bool ballWon = false;

	private Rigidbody ballBody;
	private TargetMover ballMover;
	private TargetMover targetMover;

	private float viewDistance = 3;

	private VectorSensorComponent goalSensor;

	public override void Initialize()
    {
		base.Initialize();

		p.target = ball;
		ballMover = ball.GetComponent<TargetMover>();
		targetMover = target.GetComponent<TargetMover>();

		ballBody = ball.GetComponent<Rigidbody>();

		goalSensor = GetComponent<VectorSensorComponent>();
	}

	public override void CalculateRewards()
	{
		base.CalculateRewards();

		GetErrorVelocity(GetBallError(), ballBody.velocity, out float ballError, out float ballSpeed);

		float ballDamper = ballSpeed * ballDamperReward;
		float ballSpring = -ballError / 10f * ballSpringReward;
		float ballVelocity = ballBody.velocity.magnitude * ballVelocityReward;

		RecordStat("Soccer/Ball Spring Error", ballError, Histogram);
		RecordStat("Soccer/Ball Damper Speed", ballSpeed, Histogram);
		RecordStat("Soccer/Ball Spring Reward", ballSpring, Histogram);
		RecordStat("Soccer/Ball Damper Reward", ballDamper, Histogram);
		RecordStat("Soccer/Ball Velocity", ballBody.velocity.magnitude, Histogram);
		RecordStat("Soccer/Ball Velocity Reward", ballVelocity, Histogram);

		float reward = 0;

		reward += ballDamper;
		reward += ballSpring;
		reward += ballVelocity;

		AddReward(reward);
	}


	public override void CollectObservations(VectorSensor sensor)
	{
		base.CollectObservations(sensor);

		sensor = goalSensor.GetSensor();

		var targetPosition = referential.InverseTransformNormalizedPoint(target.position, viewDistance);
		sensor.AddObservation(Horizontal2(targetPosition));
		var ballPosition = referential.InverseTransformNormalizedPoint(ball.position, viewDistance);
		sensor.AddObservation(Horizontal2(ballPosition));

		var velocity = referential.InverseTransformVector(ballBody.velocity);
		sensor.AddObservation(Horizontal2(velocity));
		//var angularVelocity = referential.InverseTransformVector(ballBody.angularVelocity);
		//sensor.AddObservation(Horizontal(angularVelocity));

		/*
		var rightFoot = referential.InverseTransformNormalizedVector(p.rightFoot.transform.position - ball.position, viewDistance);
		sensor.AddObservation(rightFoot);
		var leftFoot = referential.InverseTransformNormalizedVector(p.leftFoot.transform.position - ball.position, viewDistance);
		sensor.AddObservation(leftFoot);
		*/

	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();

		BallWinCondition();
	}

	public void BallWinCondition()
	{
		// If ball is close enough, win
		if((ball.position - target.position).sqrMagnitude < ballWinDistance*ballWinDistance)
		{
			AddReward(ballWinReward);

			ballWon = true;

			EndEpisode();
		}
	}

	public override void OnEpisodeEnd()
	{
		base.OnEpisodeEnd();

		RecordStat("Soccer/Won", ballWon ? 1f : 0f);

		ballWon = false;
	}
	

	public override void ArcUpdate()
	{

	}

	public override void SetTarget()
	{
		if (targetMover && ballMover && p.training && !p.recording)
		{
			Vector3 targetPosition = p.head.transform.position;
			targetPosition.y = ballRadius;
			ballBody.velocity = Vector3.zero;
			ballBody.angularVelocity = Vector3.zero;
			if (Randomizing() && p.randomTarget)
			{
				ballMover.randomRadius(targetPosition, 3, 10);
				targetMover.randomRadius(ball.position, 5, 10);
			}
			else
			{
				ball.position = targetPosition + Vector3.forward * 5;
				target.position = ball.position + Vector3.forward * 5;
			}
		}
	}

	public Vector3 GetBallError()
	{
		Vector3 errorVector = target.position - ball.position;
		errorVector.y = 0;
		return errorVector;
	}

}
