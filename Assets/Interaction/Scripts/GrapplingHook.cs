using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class GrapplingHook : MonoBehaviour
{

    private Rigidbody body;

    private GenericPlayerInput input;

    private bool triggered = false;

    public Transform aimAxis;

    public GameObject hookPrefab;

    private Hook hook;

    public float launchSpeed = 10f;

    public float tractionSpeed = 2f;

    public float ropeSpeed = 2f;

    public float ropeForce = 100f;

    private float ropeSize;

    private bool autoTract = true;

    void Start()
    {
        body = GetComponent<Rigidbody>();
        input = GenericPlayerInput.GetInput(gameObject);
    }


    private void FixedUpdate()
    {
        bool triggering = input.GetSecondaryActionButton();
        
        if(triggering && !triggered)
        {
            triggered = true;
            Shoot();
        }

        if(!triggering && triggered)
        {
            triggered = false;
            UnHook();
        }

        if(hook && hook.hooked)
        {
            Rope();
        }
        
    }

    private void Shoot()
    {
        UnHook();

        GameObject hookObject = Instantiate(hookPrefab, aimAxis.position, aimAxis.rotation);

        /*
        hookObject.layer = gameObject.layer;
        foreach (Transform trans in hookObject.GetComponentsInChildren<Transform>(true))
        {
            trans.gameObject.layer = gameObject.layer;
        }
        */



        hook = hookObject.GetComponent<Hook>();
        hook.grapplingHook = this;

        Rigidbody hookBody = hook.GetComponent<Rigidbody>();
        hookBody.velocity = aimAxis.forward * launchSpeed;

    }
    public void Hook()
    {
        autoTract = true;
        ropeSize = Vector3.Distance(aimAxis.position, hook.transform.position);
    }

    public void Rope()
    {

        float distance = Vector3.Distance(aimAxis.position, hook.transform.position);

        float control = input.GetControlAxis();
        if (control != 0) autoTract = false;
        if (autoTract) control = 1;
        if (control != 0)
        {
            ropeSize = Mathf.Max(0, distance - Mathf.Sign(control) * tractionSpeed);
        }

        if (distance > ropeSize)
        {

            Vector3 norm = (hook.transform.position - aimAxis.position) / distance;

            float speed = Vector3.Dot(norm, body.velocity);

            float error = distance - ropeSize;

            float forceMagnitude = ropeForce * (ropeSpeed * error - speed);

            Vector3 force = norm * forceMagnitude;

            if (Utils.IsFinite(force) && forceMagnitude > 0)
            {
                body.AddForce(force); 
                hook.body.AddForce(-force);
            }

        }

    }

    private void UnHook()
    {
        if (hook)
        {
            Destroy(hook.gameObject);
            hook = null;
        }
    }

}
