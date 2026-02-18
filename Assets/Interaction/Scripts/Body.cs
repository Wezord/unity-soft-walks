using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Body : MonoBehaviour
{
    
    private GenericPlayer player;

    private CapsuleCollider capsule;

    [Range(0, 100)] public float bodySize = 30.0f;

    private bool initialized = false;

    private float realHeight;

    public void Initialize(GenericPlayer player)
    {
        this.player = player;

        if (initialized) return;

        capsule = GetComponent<CapsuleCollider>();

        realHeight = player.GetHeight();

        //configureCapsule();

        initialized = true;
    }

    /*
    public void Update()
    {
        configureCapsule();
    }
    */

    public void FixedUpdate()
    {
        configureCapsule();
    }

    private void configureCapsule()
    {

        Vector3 position = transform.localPosition;
        position.y = -realHeight / 2;
        transform.localPosition = position;

        capsule.height = player.GetHeight() * bodySize / 100.0f;

        capsule.center = new Vector3(0, (realHeight - capsule.height) / 2.0f, 0);
    }

    public Vector3 getBottom()
    {
        return new Vector3(0, capsule.center.y - capsule.height / 2.0f, 0);
    }

    public void SetHeight(float height)
    {
        realHeight = height;
    }

}
