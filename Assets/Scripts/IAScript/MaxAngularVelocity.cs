using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaxAngularVelocity : MonoBehaviour
{

    public float maxAngularVelocity = 100.0f;

    // Start is called before the first frame update
    void Start()
    {
        Rigidbody body = GetComponent<Rigidbody>();
        if (body) body.maxAngularVelocity = maxAngularVelocity;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
