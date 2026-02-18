public abstract class DamperPenalty : RewardProvider
{
	public float multiplier = 1f;

	public override void AddRewards(HumanBotAgent agent, HumanBotParameters p)
	{
		agent.AddReward(-p.root.body.velocity.magnitude * multiplier);
	}

}
