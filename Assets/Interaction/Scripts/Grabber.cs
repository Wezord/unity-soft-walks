using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Grabber : MonoBehaviour
{
    [NonSerialized]
    public bool grabbing;
    [NonSerialized]
    public bool grabbed = false;
    public bool canGrab = true;
    public bool freeRotation = true;

    private Rigidbody body;

	[NonSerialized]
	public ConfigurableJoint grabJoint;
	[NonSerialized]
	public GameObject grabbedObject;
    private List<Collider> grabbedColliders;

	//public AK.Wwise.Event grabSoundEvent;
	//public AK.Wwise.Event unGrabSoundEvent;

    public PlayerType playerType;

    [NonSerialized]
    public Collider[] colliders;

    private float timeout = 0.1f;

	[NonSerialized]
	public Quaternion startLocalRotation;

    private bool lockRotation = true;

	public void Start()
    {
		body = GetComponent<Rigidbody>();
        colliders = GetComponentsInChildren<Collider>();
		grabbedColliders = new List<Collider>();
	}


	public void Grab(Collision collision)
    {

		if (!canGrab || !grabbing || grabbed)
            return;

        var tag = collision.gameObject.tag;

        if (tag.Equals("Ungrabbable") || tag.Equals("Weapon"))
            return;

		grabbed = true;
		grabbedObject = collision.gameObject;
		//grabbedLayer = grabbedObject.layer;

		Events.SignalPreGrab(gameObject, collision.gameObject, playerType);

		TempDisableCollisions(collision.GetContact(0).otherCollider);

		/*
		grabbedObject.layer = LayerMask.NameToLayer("Grabbed");
		Physics.IgnoreLayerCollision(gameObject.layer, grabbedObject.layer, true);
		Debug.Log("Grabbed " + collision.gameObject);
        */

        startLocalRotation = transform.localRotation;

		grabJoint = gameObject.AddComponent<ConfigurableJoint>();
        
        grabJoint.enablePreprocessing = false;
		grabJoint.enableCollision = false;
		//grabJoint.projectionMode = JointProjectionMode.PositionAndRotation;
		grabJoint.xMotion = ConfigurableJointMotion.Locked;
        grabJoint.yMotion = ConfigurableJointMotion.Locked;
        grabJoint.zMotion = ConfigurableJointMotion.Locked;
        var rotMotion = freeRotation ? ConfigurableJointMotion.Free : ConfigurableJointMotion.Locked;
		grabJoint.angularXMotion = rotMotion;
        grabJoint.angularYMotion = rotMotion;
        grabJoint.angularZMotion = rotMotion;
        

        grabJoint.anchor = transform.InverseTransformPoint(collision.contacts[0].point);

        if (collision.rigidbody)
        {
            grabJoint.connectedBody = collision.rigidbody;
            //grabJoint.connectedMassScale = collision.rigidbody.mass / body.mass;
        }
        else if (collision.articulationBody)
        {
            grabJoint.connectedArticulationBody = collision.articulationBody;
        }

        grabbedObject.AddComponent<Grabbed>().grabber = this;

        SetRotationLock(lockRotation);

		//grabSoundEvent.Post(gameObject);

		Events.SignalGrab(gameObject, collision.gameObject, playerType);

	}

	public void Ungrab()
    {
        if (!canGrab) return;
        
        if (grabJoint != null)
        {
			StartCoroutine(TempReenableCollisions());

			Events.SignalUnGrab(gameObject, grabbedObject, playerType);

			DestroyImmediate(grabJoint);
            grabJoint = null;
            //unGrabSoundEvent.Post(gameObject);
		}

        Grabbed grabbedC = grabbedObject.GetComponent<Grabbed>();
        if(grabbedC != null)
        {
            DestroyImmediate(grabbedC);
        }

        grabbed = false;
        grabbing = false;
        grabbedObject = null;


		/*
		Debug.Log("Ungrabbed " + grabbedObject);

		Physics.IgnoreLayerCollision(gameObject.layer, grabbedObject.layer, false);
        grabbedObject.layer = grabbedLayer;
        */

	}

    public void SetRotationLock(bool lockRot)
    {
        if(grabJoint != null)
        {
            if (lockRot)
            {
                grabJoint.angularXMotion = ConfigurableJointMotion.Locked;
                grabJoint.angularYMotion = ConfigurableJointMotion.Locked;
                grabJoint.angularZMotion = ConfigurableJointMotion.Locked;
            }
            else
            {
                grabJoint.angularXMotion = ConfigurableJointMotion.Free;
                grabJoint.angularYMotion = ConfigurableJointMotion.Free;
                grabJoint.angularZMotion = ConfigurableJointMotion.Free;
            }
            if(lockRot != lockRotation)
            {
                if (lockRot)
                {
                    grabJoint.axis = grabJoint.axis;
                    grabJoint.secondaryAxis = grabJoint.secondaryAxis;
                }
            }
        }
        lockRotation = lockRot;
    }

    void OnCollisionStay(Collision collision)
    {
        Grab(collision);
    }

	public void SetGrabbing(bool g = true) {
        grabbing = g;

        if (grabbed && !grabbing)
        {
            Ungrab();
        }
    }

    public void OnDestruction(GameObject obj)
    {
        if (!canGrab || !grabbed || grabJoint == null) return;

        GameObject connectedObject;
        if(grabJoint.connectedBody != null)
        {
            connectedObject = grabJoint.connectedBody.gameObject;
        } else if (grabJoint.connectedArticulationBody != null)
        {
			connectedObject = grabJoint.connectedArticulationBody.gameObject;
		} else
        {
            return;
        }

        do
        {
			if (obj == connectedObject)
			{
				Ungrab();
				return;
			}
            var parent = connectedObject.transform.parent;
            if (parent == null) return;
			connectedObject = parent.gameObject;
		} while (connectedObject != null);
    }

	public void TempDisableCollisions(Collider collider)
	{
		grabbedColliders.Add(collider);

		foreach (var grabberCollider in colliders)
		{
			Physics.IgnoreCollision(grabberCollider, collider, true);
            grabberCollider.isTrigger = true;
		}
	}

	public IEnumerator TempReenableCollisions()
    {
        var grabbedCollidersCopy = grabbedColliders.ToArray();
		grabbedColliders.Clear();

		yield return new WaitForSeconds(timeout);

		foreach (var grabbedCollider in grabbedCollidersCopy)
        {
			if (!grabbedCollider) continue;
		    foreach (var grabberCollider in colliders)
			{
                if (!grabberCollider) continue;
				Physics.IgnoreCollision(grabberCollider, grabbedCollider, false);
                grabberCollider.isTrigger = false;
			}
		}
	}

	private void OnEnable()
	{
		Events.destroyEvent += OnDestruction;
	}
    
    private void OnDisable()
	{
		Events.destroyEvent -= OnDestruction;
	}

}
