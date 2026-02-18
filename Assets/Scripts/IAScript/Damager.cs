using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damager : MonoBehaviour
{
    /*
    public float interpolation;
    public float dangerZone;
    public float hitZone;
    public float hitCooldown;
    public float timeScale;

    public Transform head;
    private Health target;

    public Rigidbody body;
    
    public double lastHit;
    public void Start()
    {
        target = head.gameObject.GetComponent<Health>();
        lastHit = Time.timeAsDouble;

        if(body == null) body = GetComponent<Rigidbody>();
    }

    public void FixedUpdate()
    {
        if (target == null) return;

        var distance = Vector3.Distance(transform.position + (body.velocity * interpolation * Time.fixedDeltaTime), head.position);

        var now = Time.timeAsDouble;
        if (now - lastHit > hitCooldown)
        {
            if (distance < hitZone)
            {
                lastHit = now;
                target.Hit(1);
            }
            else if (distance < dangerZone)
            {
                //target.Danger();
                Time.timeScale = (distance / dangerZone) * (1 - timeScale) + timeScale;
                return;
            }
        }
        //target.NoDanger();
        Time.timeScale = 1;
    }
    */
}

