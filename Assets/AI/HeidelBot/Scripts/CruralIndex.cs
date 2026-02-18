using UnityEngine;

[ExecuteAlways]
public class CruralIndex : MonoBehaviour
{
	public Length leftFemur, leftTibia, rightFemur, rightTibia;
	[Range(60f, 100f)]
    public float cruralIndex = 90f;
	public float currentCruralIndex = 0f;

    public bool scale = false;
    public bool reset = true;

    [SerializeField]
    private double sum = 0;

    void Update()
    {
		if (!IsActive()) return;

		Scale(leftTibia, leftFemur);
		Scale(rightTibia, rightFemur);

	}

	public void Scale(Length tibia, Length femur)
	{
		tibia.reset = reset;
		femur.reset = reset;
		tibia.scale = scale;
		femur.scale = scale;


		if (reset)
		{
			sum = tibia.Get() + femur.Get();
			reset = false;
		}

		if (scale)
		{
			/*
			cruralIndex = tibia / femur * 100
			sum = tibia + femur

			tibia = sum - femur
			cruralIndex = (sum - femur) / femur * 100
			
			cruralIndex / 100 = (sum - femur) / femur = sum/femur - 1
			cruralIndex / 100 + 1 = sum/femur
			femur = sum / (cruralIndex / 100 + 1)
			*/

			femur.targetSize = sum / (cruralIndex / 100 + 1);
			tibia.targetSize = sum - femur.targetSize;
		}

		currentCruralIndex = (float) (tibia.Get() / femur.Get() * 100);

	}

	public bool IsActive()
	{
		return leftFemur && leftTibia && rightFemur && rightTibia && enabled && !Application.isPlaying;
	}
}
