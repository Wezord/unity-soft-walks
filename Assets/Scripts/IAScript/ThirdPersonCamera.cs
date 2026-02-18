using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{

	public new Camera camera;
	public Transform target;

	public float rotationSpeed = 1f;
	public float distance = 5f;

	public float filtering = 1f;
	public float maxDistance = 0f;

	public float minZoom = 1f, maxZoom = 20f;

	private GenericPlayerInput input;

	public Vector2 rotations = Vector2.zero;

	private Vector3 staticCameraCenter;
	private Vector3 targetPosition;

	void Start()
    {
		input = GenericPlayerInput.GetInput(gameObject);

		Cursor.lockState = CursorLockMode.Locked;
		targetPosition = target.position;
		staticCameraCenter = target.position;

	}

    void LateUpdate()
    {
		distance -= input.GetControlAxis() * 20f / 60f;
		distance = Mathf.Clamp(distance, minZoom, maxZoom);

		rotations += input.GetViewAxis() * rotationSpeed / 60f;
		
		float e = 0.001f;
		rotations.y = Mathf.Clamp(rotations.y, -Mathf.PI/2+e, Mathf.PI/2-e);

		var offset = new Vector3(Mathf.Cos(-rotations.x), 0, Mathf.Sin(-rotations.x));

		offset *= Mathf.Cos(-rotations.y);
		offset.y = Mathf.Sin(-rotations.y);

		var dist = Vector3.Distance(staticCameraCenter, target.position);
		if (dist > maxDistance)
		{
			targetPosition = target.position;
			staticCameraCenter = target.position;
		}

		targetPosition = Vector3.MoveTowards(targetPosition, target.position, Vector3.Distance(targetPosition, target.position) * filtering);
		camera.transform.position = staticCameraCenter + offset * distance;

		camera.transform.LookAt(targetPosition);
	}
}
