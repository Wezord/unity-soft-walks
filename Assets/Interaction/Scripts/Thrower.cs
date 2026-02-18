using UnityEngine;

public class Thrower : MonoBehaviour
{

    public float velocityMultiplier = 1f, angularVelocityMultiplier = 1f;

    private Grabber grabber;
    private Hand hand;

    void Start()
    {
        grabber = GetComponent<Grabber>();
        hand = GetComponent<Hand>();
    }

    private void OnUnGrab(GameObject grabber, GameObject obj, PlayerType type)
    {
        if(grabber == gameObject)
        {
            var rb = obj.GetComponent<Rigidbody>();
			if (rb)
            {
				//rb.position = hand.player.transform.TransformPoint(hand.position - hand.player.position);
				//rb.rotation = hand.player.transform.rotation * hand.rotation;
				rb.velocity = hand.player.body.velocity + hand.player.transform.TransformVector(hand.velocity);
                rb.velocity *= velocityMultiplier;
                rb.angularVelocity *= angularVelocityMultiplier;
				//rb.angularVelocity = hand.angularVelocity;

			}
        }
    }

	private void OnEnable()
	{
		Events.ungrabEvent += OnUnGrab;
	}
}
