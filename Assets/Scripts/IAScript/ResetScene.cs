using UnityEngine;

public class ResetScene : MonoBehaviour
{

	private bool hasReset = false;

    void Start()
    {
        Events.destroyEvent += ResetAll;
	}

	private void FixedUpdate()
	{
		hasReset = false;
	}

	void ResetAll(GameObject obj)
    {
		if (hasReset) return;
		hasReset = true;
		var resetters = FindObjectsOfType<Resetter>();
        foreach (var resetter in resetters)
        {
			resetter.ResetTransform();
		}
	}
}
