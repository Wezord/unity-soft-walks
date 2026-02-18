using UnityEngine;

public class AutomaticBotController : MonoBehaviour
{

    public HumanBotAgent agent;

    void Awake()
    {
        // Find first human bot agent
        if(!agent) agent = FindObjectOfType<HumanBotAgent>();

        // Set camera target to agent root
        var cameraController = GetComponent<ThirdPersonCamera>();
        if (cameraController && cameraController.target == null) cameraController.target = agent.p.root.transform;

        // Set agent parameters for bot controller and arrow controller
		var botController = GetComponent<HumanBotController>();
		if (botController && botController.p == null) botController.p = agent.p;
		var arrowController = GetComponent<BotArrowController>();
		if (arrowController && arrowController.p == null) arrowController.p = agent.p;
	}
}
