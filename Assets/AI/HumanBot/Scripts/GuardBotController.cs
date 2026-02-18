using UnityEngine;

using static BotAgent;

public class GuardBotController : MonoBehaviour
{

    public float radius = 3f;

	public LayerMask potentialCriminals;

	public Renderer[] renderers;
	public Material passiveEyes, aggressiveEyes;

	//public AK.Wwise.Event alertEvent;

    HumanBotAgent agent;
    HumanBotParameters p;

    private float speed;

	private bool aggressive = false;

    void Start()
    {
        agent = GetComponent<HumanBotAgent>();
        p = GetComponent<HumanBotParameters>();
        speed = p.targetSpeed;

		foreach(Rigidbody rb in agent.GetComponentsInChildren<Rigidbody>())
		{
			var hc = rb.gameObject.AddComponent<HitCrime>();
			hc.potentialMask = potentialCriminals;
		}
    }

    void FixedUpdate()
    {
        if(!aggressive) StandNearTarget(radius);
	}

    void StandNearTarget(float radius)
    {
		if (Horizontal(p.root.transform.position - p.target.position).magnitude < radius)
		{
			p.targetSpeed = 0;
		}
		else
		{
			p.targetSpeed = speed;
		}
	}

	private void OnCrime(Vector3 position, GameObject crimer, GameObject crimee, Events.CrimeType type, int level, float sound)
	{
		//if (!aggressive) alertEvent.Post(p.head.gameObject);
		p.target = crimer.transform;
		p.targetSpeed = speed;
		foreach(Renderer renderer in renderers)
		{
			renderer.material = aggressiveEyes;
		}
		aggressive = true;
	}

	private void OnDestruction(GameObject obj)
	{
		if (obj == gameObject)
		{
			foreach (Renderer renderer in renderers)
			{
				renderer.material = passiveEyes;
			}
			aggressive = false;
			p.target = p.root.transform;
			p.targetSpeed = 0;
		}
	}

	private void OnEnable()
	{
		var surveillance = GetComponent<Surveillance>();
		if(surveillance) surveillance.witnessEvent += OnCrime;
		Events.destroyEvent += OnDestruction;
	}

	private void OnDisable()
	{
		var surveillance = GetComponent<Surveillance>();
		if (surveillance) surveillance.witnessEvent -= OnCrime;
		Events.destroyEvent -= OnDestruction;
	}

}
