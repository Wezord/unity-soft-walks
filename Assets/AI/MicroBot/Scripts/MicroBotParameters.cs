using UnityEngine;

public class MicroBotParameters : BotParameters
{

	[Header("Micro Bot Parameters")]

	[Header("Reward Factors")]
	public float headReward = 1f;
	public float rootReward = 1f;

	[Header("Win")]
	public int arcFrames = 200;

	[Header("Death")]
	public bool dieOnFall = false;
	public int maxDyingFrames = 0;
	public float dyingHeightFactor = 0.6f;

	[Header("Body Parts")]
	public Rigidbody root;
	public Rigidbody head;
	public Transform[] feet;

}
