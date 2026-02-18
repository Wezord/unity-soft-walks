using UnityEngine;

public class SlopeTraining : MonoBehaviour
{

    public HumanBotAgent agent;
    public float minSlope = -45f;
    public float maxSlope = 45f;
    private Quaternion baseAxis;    

    void Awake()
    {
        AgentEvents events = agent.GetComponent<AgentEvents>();
        events.beginEpisodeEvent += OnEpisodeBegin;
        baseAxis = transform.rotation;
    }

    void OnEpisodeBegin()
    {
        float slope = Random.Range(minSlope, maxSlope);
        transform.rotation = baseAxis * Quaternion.AngleAxis(slope, Vector3.forward);
    }
}
