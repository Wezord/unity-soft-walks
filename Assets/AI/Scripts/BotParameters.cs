using System;
using Unity.MLAgents;
using UnityEngine;

public class BotParameters : MonoBehaviour
{

	[Header("Training")]
	// Max amount of FixedUpdates (60Hz) in an episode
	// After reaching the end, the episode is truncated,
	// where the value function estimates future rewards from the current observations,
	// and uses it for training.
	// 0 means an episode can go on forever
	public int maxFrames = 0;
	// Training mode that affects certain functionalities
	// Necessary for training, and optional for inference or deployment
	// False means no randomizing will happen, with other side-effects
	public bool training = false;
	
	[Header("Randomization")]
	// Whether to apply any of the randomizing functions
	// false means no randomizing at all
	// true means other types of random will be applied if they are also true
	public bool randomizing = false;
	// Changes the target every HumanBotParameters.arcUpdates FixedUpdates
	// Moves the target in a random horizontal circle around the agent
	// at a distance between 20 and 30 meters
	// If False, the target will be in front of the agent in the global Z direction, guiding it in a straight line
	public bool randomTarget = false;

	public bool randomStartInit = false;

	
	// Sometimes will start an episode by placing the agent with a random rotation in the air
	// Happens with a probability of (randomInitChance * 100)% at every episode begin
	// Necessary to learn to get back up, but needs a HumanBotParameters.maxDyingFrames at least 60 (1 sec) for it to be able to get up in time 
	public bool randomInit = false;
	// Randomly applies forces and torques to a random body part
	// Happens with a probability of (pushChance * 100)% at every FixedUpdate
	// Force and torques are random between 0 and pushForce
	public bool pushing = false;
	// Randomly applies a joint to a random body part, pulling it to a specific position and rotation
	// Happens with a probability of (pullChance * 100)% at every FixedUpdate
	// Joint drive forces are random between 0 and pullForce
	// Joint is destroyed after up to a second
	public bool pulling = false;
	// Randomly picks a target speed from speeds[]
	// Happens every HumanBotParameters.arcUpdates FixedUpdates
	public bool randomSpeed = false;
	// Defines the possible target speeds that are possible
	// Also defines the speeds of the animation controllers that are in ImitationRecorder (subject to change)
	public float[] speeds;

	[Header("Randomization Parameters")]
	// Probability of randomInit per episode begin
	public float randomInitChance = 0.2f;
	// Probability of pushing per frame
	public float pushChance = 0.001f;
	// Push force/torque multiplier
	public float pushForce = 1f;
	// Probability of pulling per frame
	public float pullChance = 0.001f;
	// Pull joint drive force/torque multiplier
	public float pullForce = 1f;

	[Header("Behavior")]
	// Prepares the agent for recording demonstrations
	// Automatically set in ImitationRecorder
	public bool recording = false;
	// Sets the character to kinematic animation mode, applying animator's controller
	// Automatically set in ImitationRecorder
	public bool animating = false;
	// Sets rigidbodies to not be able to move or rotate on start
	// Useful for debugging and unfreezing only certains parts
	public bool freezing = false;
	// Kinematic aka not subject to the physics engine, rather subject to animating
	public bool kinematic = false;
	// All joint drive targets are 0
	public bool immobile = false;
	// Period of FixedUpdates for calling a policy inference
	// Calls the neural network engine evert decisionPeriod FixedUpdates
	public int decisionPeriod = 5;

	[Header("Observations")]
	// Substracts the parent joint velocity to a child joint's velocity for the velocity observation
	// Results in the possibility for an agent to learn a skill backwards (walking backwards while imitating a forward walk animation)
	public bool relativeVelocity = true;
	// Whether to observe in the parent joints space or the root body space
	public bool jointSpace = false;
	// Whether to multiply the velocity observations by the radius difference between the base humanoid limb and a resized limb
	// Difference is small
	public bool normalizedVelocity = true;

	[Header("Reward")]
	public float rewardFactor = 1f;
	public float winDistance = 0.5f;
	public float energyPenalty = 1f;
	public float winReward = 1000f;
	public float deathPenalty = -1000f;
	public float collisionPenalty = 1f;
	public float targetSpeed = 1.37f;

	[Header("Mass")]
	public float totalMass = 80f;
	public float bodyPercentage = 0f;

	[Header("Forces")]
	public float torqueFactor = 1f;
	public float rotationSpring = 100f;
	public float rotationDamper = 10f;
	public float angularVelocityFactor = 0f;
	public float maxTorque = 1000f;
	public float maxAngularVelocity = 10;
	public bool springControl = true;

	public float breakTorque = float.PositiveInfinity;
	public float breakForce = float.PositiveInfinity;
	public bool selfColliding = true;

	[Header("Components")]
	public Transform target;
	[NonSerialized]
	public TargetMover targetMover = null;
	public Env environment = null;

	[NonSerialized]
	public BotAgent agent;
	public Animator animator = null;

}
