using UnityEngine;

[ExecuteAlways]
public class Length : MonoBehaviour
{

	[Min(0f)]
	public double realSize = 0f;

	[Min(0f)]
	public double targetSize = 0f;

	public bool scale = false;
	public bool reset = false;

	[SerializeField]
	private Vector3 startPosition = Vector3.zero;

	public Transform start, end;

	private ConfigurableJoint joint;
	public Collider collider;

	private void Start()
	{
		startPosition = transform.localPosition;
		joint = GetComponent<ConfigurableJoint>();
	}

	private void Update()
	{
		if(reset)
		{
			//Rescale(1.0 / ((transform.localScale.x+transform.localScale.y+transform.localScale.z)/3.0));
			transform.localPosition = startPosition;
			SetRealSize(Get());
			SetTargetSize(realSize);
			reset = false;
		}

		if(IsActive())
		{
			Calculate();

			if(realSize == 0)
			{
				Calculate();
			}

			if(targetSize == 0)
			{
				SetTargetSize(realSize);
			}

			if (scale)
			{
				Rescale(targetSize / realSize);
				Calculate();
				scale = false;
			}
		}
	}

	private double Calculate()
	{
		realSize = Get();
		return realSize;
	}

	private void SetTargetSize(double Size)
	{
		targetSize = Size;
	}
	private void SetRealSize(double Size)
	{
		realSize = Size;
	}

	public void Rescale(double scale)
	{
		if (scale <= 0f) return;
		transform.localPosition *= (float) scale;
		//UpdateCollider(scale);
		UpdateJoint();
	}

	public double Get()
	{
		return IsActive() ? Vector3.Distance(end.position, start.position) : realSize;
	}
	public bool IsActive()
	{
		return end && start && enabled && !Application.isPlaying;
	}

	public void UpdateJoint()
	{
		if (joint && joint.connectedBody)
		{
			joint.autoConfigureConnectedAnchor = false;
			joint.autoConfigureConnectedAnchor = true;
			//joint.anchor = Vector3.zero;
			//joint.connectedAnchor = joint.connectedBody.transform
		}
	}

	/*
	public void UpdateCollider(double scale)
	{
		if (!collider) return;

		if(collider is CapsuleCollider)
		{
			var cap = (CapsuleCollider) collider;
			cap.height *= (float) scale;
		}
	}
	*/

}
