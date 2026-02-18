using UnityEngine;

public class VRBotController : MonoBehaviour
{

	public Transform head, rightHand, leftHand;

	private BotParameters p;
	private BotAgent agent;

	private Transform rightProxy;
	private PositionController rightHandController;

    void Start()
    {
		p = GetComponent<HumanBotParameters>();
		agent = GetComponent<HumanBotAgent>();

		var events = GetComponent<AgentEvents>();

		events.beginEpisodeEvent += OnEpisodeBegin;

		rightProxy = new GameObject("Right Hand Target").transform;

		rightHandController = GetComponent<PositionController>();

		rightHandController.target = rightProxy;

		p.target = head;

	}

	void OnEpisodeBegin()
    {
		var pos = head.position;
		pos.y = transform.position.y;
		transform.position = pos;
    }

	private void FixedUpdate()
	{
		var center = rightHandController.targetCenter.position;
		var diff = rightHand.position - center;
		var dist = diff.magnitude;
		if(dist > rightHandController.targetRadius)
		{
			diff *= rightHandController.targetRadius / dist;
		}
		rightProxy.position = center + diff;
	}

}
