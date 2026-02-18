using Unity.Barracuda;
using Unity.MLAgents.Policies;
using UnityEngine;

public class ModelSwitch : MonoBehaviour
{
    public NNModel liveModel, dyingModel;

    private HumanBotAgent agent;
    private HumanBotParameters p;
    private BehaviorParameters behavior;

    public float dyingHeightFactor = 0.7f;

    public float timeToModelChange = 0.5f;
    private float timeCounter = 0f;

    private bool wasDying = false;
    private bool dyingModelSet = false;

    void Start()
    {
        agent = GetComponent<HumanBotAgent>();
		behavior = GetComponent<BehaviorParameters>();
		p = GetComponent<HumanBotParameters>();

	}

    void FixedUpdate()
    {
        var dying = agent.GetCurrentHeight() < dyingHeightFactor * p.height;
        
		if (dyingModelSet != dying)
        {
            timeCounter += Time.fixedDeltaTime;
            if(timeCounter >= timeToModelChange) {
                agent.SetModel(behavior.BehaviorName, dying ? dyingModel : liveModel, behavior.InferenceDevice);
                dyingModelSet = dying;
                timeCounter = 0f;
            }
		} else {
            timeCounter = 0f;
        }
        wasDying = dying;
    }
}
