using UnityEngine;

public class AutoGrab : MonoBehaviour
{

	public Transform target;
	public Transform grabPoint;

	public bool locked = false;

	public float spring = 1f, damper = 1f;

	private Rigidbody targetBody;
	private Grabber grabber;

	private void Start()
	{
		grabber = GetComponent<Grabber>();
		if(target) targetBody = target.GetComponent<Rigidbody>();
	}

	public void FixedUpdate()
	{
		if(grabber.grabbing && !grabber.grabbed)
		{
			if (target && targetBody && !target.GetComponent<Grabbed>())
			{
				Vector3 force = spring * (grabPoint.position - target.position) - damper * targetBody.velocity;
				targetBody.AddForce(force);
			}
		}
    }

	/*
	public IEnumerator MoveTarget()
	{
		while(grabber.grabbing && !grabber.grabbed)
		{
			
			yield return new WaitForFixedUpdate();
		}
		Debug.Log("Done");
	}
	*/

	
	private void OnGrab(GameObject grabber, GameObject grabbedObject, PlayerType playerType)
	{
		if (grabber != gameObject || locked) return;
		var rb = grabbedObject.GetComponent<Rigidbody>();
		if(rb)
		{
			target = grabbedObject.transform;
			targetBody = rb;
		}
	}

	private void OnEnable()
	{
		Events.grabEvent += OnGrab;
	}

	private void OnDisable()
	{
		Events.grabEvent -= OnGrab;
	}
}
