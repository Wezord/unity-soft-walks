using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class ConfigureParameters : MonoBehaviour
{

    public Parameters parameters;

    public bool rightTrigger = true;

    public float v1 = 1.0f;
    public float v2 = 1.0f;
    public float v3 = 1.0f;
    public float v4 = 1.0f;

    void Update()
    {
        if (!VRPlayerInput.configuring) return;

        if ((rightTrigger && VRPlayer.player.rightHand.device.query(CommonUsages.triggerButton, out bool rightTriggered) && rightTriggered)
            || (!rightTrigger && VRPlayer.player.leftHand.device.query(CommonUsages.triggerButton, out bool leftTriggered) && leftTriggered)
            )
        {

            VRPlayer.player.leftHand.device.query(CommonUsages.primary2DAxis, out Vector2 leftAxis);
            
            parameters.p1 += v1 * leftAxis.y;
            parameters.p3 += v3 * leftAxis.x;

            VRPlayer.player.rightHand.device.query(CommonUsages.primary2DAxis, out Vector2 rightAxis);

            parameters.p2 += v2 * rightAxis.y;
            parameters.p4 += v4 * rightAxis.x;

        }

    }

    private void Print()
    {
        //if (!VRPlayerInput.configuring) return;

        Debug.Log(parameters.p1 + " " + parameters.p2 + " " + parameters.p3 + " " + parameters.p4);
    }
    private void OnApplicationQuit()
    {
        Print();
    }

    void OnApplicationFocus(bool status)
    {
        if(!status) Print();
    }

}
