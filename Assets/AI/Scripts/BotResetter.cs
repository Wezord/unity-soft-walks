public class BotResetter : Resetter
{

    private BotAgent agent;

    void Start()
    {
		agent = GetComponent<BotAgent>();
	}

    public override void ResetTransform()
    {
        agent.Restart();
    }
}
