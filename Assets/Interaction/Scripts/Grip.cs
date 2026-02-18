using UnityEngine;

public class Grip : MonoBehaviour
{
    private CapsuleCollider col;

    void Start()
    {
		col = GetComponent<CapsuleCollider>();
		if (col == null)
		{
			Debug.LogError("Grip script requires a CapsuleCollider component");
			return;
		}
	}

    // move fingers to grip around capsule collider
    public void ConfigureHands(Hand hand)
    {
		
	}
}
