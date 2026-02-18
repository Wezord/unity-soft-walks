using UnityEngine;

public class JointRandomizer : MonoBehaviour
{
    public float chanceToRandomize = 0.001f;

    public Transform[] joints;

    private HumanBotParameters p;

    void Start()
    {
        p = GetComponent<HumanBotParameters>();
    }

    void FixedUpdate()
    {
        RandomizeJoints();
    }

    void RandomizeJoints()
    {
        if(p.randomizing && Random.value <= chanceToRandomize) {
            foreach (Transform joint in joints)
            {
                joint.localRotation = Random.rotation;
            }
        }
    }
}
