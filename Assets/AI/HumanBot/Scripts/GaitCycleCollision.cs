using UnityEngine;

public class GaitCycleCollision : MonoBehaviour
{
	public float cycleTime = 0;
	public float minimumTime = 0.1f;

	void Start()
    {
		cycleTime = 0;
	}

    void FixedUpdate()
	{
		cycleTime += Time.fixedDeltaTime;
	}

	private void OnCollisionEnter()
	{
		if(cycleTime < minimumTime) {
			return;
		}
		cycleTime = 0;
	}

}
