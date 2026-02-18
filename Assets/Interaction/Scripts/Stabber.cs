using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stabber : MonoBehaviour
{

    public Transform tip;
    public float minSpeed = 0.1f;
    public float breakForce = 10000;
    public float breakTorque = 10000;


    private Rigidbody body;
    private ConfigurableJoint joint;
    private GameObject stabbed;
    private Vector3 collisionPosition;
    private float distance;

    private new Collider collider;
    private ContactPoint contact;

    private Vector3 startLocalPosition;

    void Start()
    {
        body = GetComponent<Rigidbody>();
        distance = Vector3.Distance(transform.position, tip.position);
        startLocalPosition = transform.localPosition;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!gameObject.activeInHierarchy) return;
        
        if (joint) {
            bool pulling = Vector3.Dot(transform.up, body.velocity) < 0;
            Ray ray = new Ray(transform.position, transform.up);
            if (pulling && !collider.Raycast(ray, out RaycastHit hitInfo, distance))
            {
                OnJointBreak(0);
                Destroy(joint);
            }
        }
    }

    public void OnJointBreak(float breakForce)
    {
        Physics.IgnoreCollision(contact.thisCollider, contact.otherCollider, false);
    }

    private void OnCollisionEnter(Collision collision)
    {

        if (!this.isActiveAndEnabled) return;

        collisionPosition = transform.localPosition;

        //if (!collision.rigidbody) return;
        if (joint != null)
        {
            if (joint.connectedBody == null)
            {
                joint = null;
            }
            else
            {
                return;
            }
        }
        //if (collision.gameObject.layer == LayerMask.NameToLayer("HumanBotAgent")) return;

        float speed = Vector3.Dot(tip.up, -collision.relativeVelocity);
        Vector3 speed3 = tip.up * speed;
        Vector3 tangent = collision.relativeVelocity + speed3;
        float otherSpeed = tangent.magnitude;

        if (speed > minSpeed)
        {
            Debug.Log("Stab");

            //Move forward slightly
            transform.position += speed3 * Time.fixedDeltaTime;

            stabbed = collision.gameObject;
            joint = gameObject.AddComponent<ConfigurableJoint>();
            joint.connectedBody = collision.rigidbody;
            joint.breakForce = breakForce;
            joint.breakTorque = breakTorque;
            joint.angularXMotion = ConfigurableJointMotion.Locked;
            joint.angularYMotion = ConfigurableJointMotion.Locked;
            joint.angularZMotion = ConfigurableJointMotion.Locked;
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Free;
            joint.zMotion = ConfigurableJointMotion.Locked;
            joint.enableCollision = false;
            var limit = joint.linearLimit;
            //limit.limit = 0.5f;

            collider = collision.collider;
            if (collision.contactCount > 0)
            {
                contact = collision.contacts[0];
                //Physics.IgnoreCollision(contact.thisCollider, contact.otherCollider);
            }

        }

    }

    private void OnCollisionExit(Collision collision)
    {
        if(joint && collision.collider == collider)
        {
            //OnJointBreak(0);
            //Destroy(joint);
            Debug.Log("broke");
        }
    }

}