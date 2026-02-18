using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorizontalCamera : MonoBehaviour
{

    public float height = 1.0f;

    private ThirdPersonCamera cam;
    private Transform target, puppet;

    void Start()
    {
        cam = GetComponent<ThirdPersonCamera>();
        target = cam.target;
		puppet = new GameObject().transform;
        puppet.position = target.position;
        cam.target = puppet;
    }

    void Update()
    {
        Vector3 pos = target.position;
		pos.y = height;
		puppet.position = pos;
	}
}
