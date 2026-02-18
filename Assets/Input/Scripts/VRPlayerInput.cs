using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class VRPlayerInput : GenericPlayerInput
{

    private Device head, leftHand, rightHand;
    public bool isConfiguring = false;
    public static bool configuring = false;

    public void Start()
    {
        head = new Device(XRNode.CenterEye);
        leftHand = new Device(XRNode.LeftHand);
        rightHand = new Device(XRNode.RightHand);
    }

    private bool leftButtonPressed = false;
    private void FixedUpdate()
    {
        if (isConfiguring)
        {
            if (leftHand.query(CommonUsages.primaryButton, out bool pressed) && pressed)
            {
                if (!leftButtonPressed)
                {
                    leftButtonPressed = true;
                    configuring = !configuring;
                }
            }
            else
            {
                leftButtonPressed = false;
            }
        }
    }

    public override bool GetJumpButton()
    {
        if(leftHand.query(CommonUsages.primary2DAxisClick, out bool pressed) && pressed)
        {
            return true;
        }
        return false;
    }

    public override Vector2 GetMovementAxis()
    {
        if (leftHand.query(CommonUsages.primary2DAxis, out Vector2 axis)
            && axis.magnitude > 0.5
            && !configuring)
        {
            return axis.normalized;
        }
        return Vector2.zero;
    }

    public override Vector2 GetViewAxis()
    {
        if (rightHand.query(CommonUsages.primary2DAxis, out Vector2 axis)
            && axis.magnitude > 0.5
            && !configuring)
        {
            return axis;
        }
        return Vector2.zero;
    }

    public override bool GetPrimaryActionButton()
    {
        if (leftHand.query(CommonUsages.triggerButton, out bool pressed) && pressed
            && !configuring)
        {
            return true;
        }
        return false;
    }

    public override bool GetSecondaryActionButton()
    {
        if (rightHand.query(CommonUsages.triggerButton, out bool pressed) && pressed
            && !configuring)
        {
            return true;
        }
        return false;
    }

    public override bool GetInteractButton()
    {
        if(rightHand.query(CommonUsages.secondaryButton, out bool pressed) && pressed)
        {
            return true;
        }
        return false;
    }

    public override bool GetWalkingButton()
    {
        if (rightHand.query(CommonUsages.primaryButton, out bool pressed) && pressed
            && !configuring)
        {
            return true;
        }
        return false;
    }

    public override float GetControlAxis()
    {
        if (rightHand.query(CommonUsages.primary2DAxis, out Vector2 axis)
            && !configuring)
        {
            return axis.y;
        }
        return 0;
    }

    public override bool IsActive()
    {
        if(!XRSettings.isDeviceActive)
        {
			return false;
		}
        if(head == null) head = new Device(XRNode.CenterEye);
		head.query(CommonUsages.userPresence, out bool present);
        return present;
    }
}
