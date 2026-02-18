using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class Phalanx : MonoBehaviour
{

    [HideInInspector]
    public Finger finger;

    [HideInInspector]
    public ArticulationBody body;

    private const float fingerSpeed = 10f;

    public bool grabbed = false;

    private Parameters param;

    // from closed to open
    public float minAngle = 0;
    public float maxAngle = 70;
    public float minMargin = 5;
    public float maxMargin = 30;

    [Range(0,1)]
    public float currentPosition = 0.0f;

    private Quaternion startRotation;

    public void Initialize(Finger finger)
    {
        this.finger = finger;
        
        startRotation = transform.localRotation;

        if (VRPlayer.ARTICULATED)
        {

            param = gameObject.AddComponent<Parameters>();
            param.p1 = 1.0f;

            /*
            ConfigureParameters config = gameObject.AddComponent<ConfigureParameters>();
            config.parameters = param;
            config.v1 = 0.01f;
            config.v2 = 0.01f;
            */

            body = gameObject.AddComponent<ArticulationBody>();
            body.mass = 0.1f;
            body.useGravity = false;
            body.jointType = ArticulationJointType.SphericalJoint;
            body.swingYLock = ArticulationDofLock.LockedMotion;
            body.swingZLock = ArticulationDofLock.FreeMotion;
            body.twistLock = ArticulationDofLock.LockedMotion;

            ArticulationDrive drive = body.zDrive;
            drive.lowerLimit = -maxAngle-maxMargin;
            drive.upperLimit = minAngle+minMargin;
            drive.stiffness = param.p1;
            drive.damping = param.p2;
            body.zDrive = drive;

        }

    }

    private void Start()
    {
        //transform.localRotation = startRotation;
    }

    void FixedUpdate()
    {

        if (VRPlayer.player.rightHand.device.query(CommonUsages.primaryButton, out bool rightTriggered) && rightTriggered)
        {
            transform.localRotation = startRotation;

        }

        if (grabbed && !finger.hand.grab.grabbing)
        {

            grabbed = false;

        }

        
        if(IsMoving()) currentPosition = Mathf.MoveTowards(currentPosition, finger.position, Time.deltaTime * fingerSpeed);
        
        
        float rotation = currentPosition * (maxAngle - minAngle) + minAngle;

        if (VRPlayer.ARTICULATED)
        {

            ArticulationDrive drive = body.zDrive;

            drive.target = -rotation;
            drive.stiffness = param.p1;
            drive.damping = param.p2;

            body.zDrive = drive;

        } else
        {
            transform.localRotation = startRotation * Quaternion.Euler(0, 0, -rotation);
        }

    }

    void OnCollisionEnter(Collision collision)
    {
        grabbed = true;
    }

    public bool IsMoving()
    {
        return finger.hand.gripping || !finger.hand.grab.grabbed;
	}

}
