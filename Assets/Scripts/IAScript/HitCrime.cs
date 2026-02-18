using UnityEngine;

public class HitCrime : MonoBehaviour
{

	public float velocityThreshold = 1f;
	public LayerMask potentialMask;
	public Events.CrimeType crimeType = Events.CrimeType.attacking;
	public int crimeLevel = 3;

	private void OnCollisionEnter(Collision collision)
	{
		// If a potential criminal
		if (potentialMask == (potentialMask | (1 << collision.gameObject.layer)))
		{
			float vel = collision.relativeVelocity.magnitude;
			if (vel > velocityThreshold)
			{
				Events.SignalCrime(transform.position, collision.gameObject, gameObject, crimeType, crimeLevel, collision.relativeVelocity.magnitude);
			}
		}
	}
}
