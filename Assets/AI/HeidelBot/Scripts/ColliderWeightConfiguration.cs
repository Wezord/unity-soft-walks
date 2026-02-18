using UnityEngine;
using UnityEngine.Rendering;

public class ColliderWeightConfiguration : MonoBehaviour
{
    [SerializeField] private float baseWeight = 70f; // Default base weight
    [SerializeField] private float currentWeight = 70f; // Current weight to compare

    public bool Apply = false;

	private void Awake()
	{
		if(Apply)
        {

			AdjustColliders();
		}
	}

    /*
	private void Update()
    {
        if (!Application.isPlaying)
        {
            if(Apply)
            {
                Apply = false;
				AdjustColliders();
			}

        }
    }
    */

    private void AdjustColliders()
    {
        float weightFactor = currentWeight / baseWeight;
        
        foreach (var capsule in GetComponentsInChildren<CapsuleCollider>())
        {
            if (capsule != null)
            {
                capsule.radius *= Mathf.Sqrt(weightFactor);
            }
        }

        foreach (var box in GetComponentsInChildren<BoxCollider>())
        {
            if (box != null)
            {
                box.size *= Mathf.Sqrt(weightFactor);
			}
        }
    }
}
