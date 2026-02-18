using UnityEngine;

public class AutonomousController : MonoBehaviour
{

    public bool followVelocity = false;
    public BotJoint velocityToFollow;

    public bool avoid = false;
	public float distanceToTurn = 5f;
	public float distanceToReverse = 1f;
    public LayerMask avoidanceLayerMask;

    private HumanBotAgent agent;
    private HumanBotParameters p;

    private Vector3 dir;

    void Start()
    {
        agent = GetComponent<HumanBotAgent>();
		p = GetComponent<HumanBotParameters>();
        dir = Vector3.right;
        p.target.position = p.root.transform.position + dir * 100f;
	}

    void FixedUpdate()
    {
        var pos = p.root.transform.position;

        if (followVelocity)
		{
            p.target.position = pos + velocityToFollow.body.velocity.normalized*1f;
        }

        if (avoid)
        {
            var ray = new Ray(pos + dir * 1.5f, dir);
            if (Physics.SphereCast(ray, 1f, out RaycastHit hit, distanceToTurn, avoidanceLayerMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.distance > distanceToReverse)
                {
                    dir = -Vector3.Cross(dir, Vector3.up);
                }
                else
                {
                    dir = -dir;
                }
                p.target.position = pos + dir * 100f;
            }
        }

        


    }
}
