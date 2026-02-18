using UnityEngine;

public class CameraFollower : MonoBehaviour
{

    public Transform target;
    public Transform position;

    void Start()
    {
        if(position == null)
        {
            position = transform;
        }

		if (target == null) return;

		transform.position = position.position;
        transform.LookAt(target);
    }

    void Update()
	{
		if (target == null) return;

		transform.position = position.position;
        Quaternion targetRotation = Quaternion.LookRotation(target.position - transform.position);
        (transform.rotation * Quaternion.Inverse(targetRotation)).ToAngleAxis(out float angle, out Vector3 axis);
        
        if(angle > 30)
        {
            transform.rotation = targetRotation;
        } else
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime);
        }
        
    }
}
