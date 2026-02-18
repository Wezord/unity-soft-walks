using UnityEngine;

public class Gripper : MonoBehaviour
{

	public Transform gripPoint;
    private Grabber grabber;
	private Hand hand;

	private Quaternion startLocalRotation;

    void Start()
    {
        grabber = GetComponent<Grabber>();
		hand = GetComponent<Hand>();
		startLocalRotation = transform.localRotation;
	}

	public void OnPreGrab(GameObject grabber, GameObject grabbedObject, PlayerType playerType)
	{
		if (playerType != PlayerType.VR) return;
		if (grabber != gameObject) return;
		var col = grabbedObject.GetComponent<CapsuleCollider>();
		if (col) // temp
		{
			PreGripCapsuleCollider(col);
		}
	}

	public void PreGripCapsuleCollider(CapsuleCollider col)
	{
		foreach (Finger finger in hand.fingers)
		{
			finger.position = 0.5f;
		}
		// wouldn't work on x or z axis capsules
		var up = gripPoint.up;
		var down = -up;

		if ((up - col.transform.up).sqrMagnitude > (down - col.transform.up).sqrMagnitude)
		{
			var temp = up;
			up = down;
			down = temp;
		}

		// gripPoint.rotation == col.transform.rotation == transform.rotation * gripPoint.localRotation
		// col.transform.rotation * Quaternion.Inverse(gripPoint.localRotation) == transform.rotation
		Quaternion localRotation = Quaternion.FromToRotation(up, col.transform.up) * transform.localRotation;
		transform.localRotation = localRotation;
		//transform.rotation = col.transform.rotation * Quaternion.Inverse(gripPoint.localRotation);
	}

	public void OnGrab(GameObject grabber, GameObject grabbedObject, PlayerType playerType)
	{
		if (playerType != PlayerType.VR) return;
		if (grabber != gameObject) return;
		if (!grabbedObject.GetComponent<Grippable>()) return;
		var col = grabbedObject.GetComponent<CapsuleCollider>();
		if(col)
		{
			GripCapsuleCollider(col);
		}
	}

	public void GripCapsuleCollider(CapsuleCollider col)
	{
		
		//grabber.grabJoint.SetTargetRotationLocal(localRotation, startLocalRotation);

		Vector3 closestPos = col.transform.position + col.transform.up * Vector3.Dot(col.transform.up, gripPoint.position - col.transform.position);
		var radius = col.transform.TransformVector(Vector3.right*col.radius).magnitude;
		
		grabber.grabJoint.autoConfigureConnectedAnchor = false;
		grabber.grabJoint.anchor = transform.InverseTransformPoint(gripPoint.position + gripPoint.right * radius);
		grabber.grabJoint.connectedAnchor = col.transform.InverseTransformPoint(closestPos);

		hand.gripping = true;
	}
	
	public void OnUnGrab(GameObject grabber, GameObject grabbedObject, PlayerType playerType)
	{
		if (playerType != PlayerType.VR) return;
		if (grabber != gameObject) return;
		if(hand.gripping)
		{
			hand.gripping = false;
		}
	}

	private void OnEnable()
	{
		Events.pregrabEvent += OnPreGrab;
		Events.grabEvent += OnGrab;
		Events.ungrabEvent += OnUnGrab;
	}

	private void OnDisable()
	{
		Events.pregrabEvent -= OnPreGrab;
		Events.grabEvent -= OnGrab;
		Events.ungrabEvent -= OnUnGrab;
	}
}
