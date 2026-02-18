using UnityEngine;

public class GaitCycle : MonoBehaviour
{
	public float cycleTime = 0;
	public float minimumTime = 0.1f;
	public float averageCycle = 0f;

	private float sumCycles = 0;
	private int numCycles = 0;

	public float offset = 0f;

	private float previousRotation;
	private float previousAngularVelocity = 0;
	private bool localMinimumDone = false;

	private float rollingAverage;
	private float oldRollingAverage;

	void Start()
    {
		cycleTime = 0;
		previousRotation = transform.localEulerAngles.x;
	}

    void FixedUpdate()
	{
		cycleTime += Time.fixedDeltaTime;

		float rotation = transform.localEulerAngles.x;
		float angularVelocity = Mathf.DeltaAngle(previousRotation, rotation);

		rollingAverage = (rollingAverage + angularVelocity)/2f;

		if (rollingAverage > offset && oldRollingAverage < 0)
		{
			if(localMinimumDone)
			{
				if(cycleTime > minimumTime)
				{
					Reset();
				}
			}
		}

		if (angularVelocity < 0 && previousAngularVelocity > 0)
		{
			localMinimumDone = true;
		}

		previousRotation = rotation;
		previousAngularVelocity = angularVelocity;

		oldRollingAverage = rollingAverage;

	}

	private void Reset()
	{
		sumCycles += cycleTime;
		numCycles++;
		averageCycle = sumCycles / numCycles;

		cycleTime = 0;

		localMinimumDone = false;
	}

}
