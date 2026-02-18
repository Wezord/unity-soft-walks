using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using System;

public class Device
{
    
    public static float deadZone = 0.5f;
    public bool enableVibration = true;

    public bool deviceFound = false;
    public InputDevice device;

    private XRNode node;

    private bool OFF = false;
    
    public Device(XRNode node)
    {

        this.node = node;

        findDevice();

    }

    public void Vibrate(float amplitude, float length)
    {
        device.SendHapticImpulse(0, amplitude, length);
    }

    public void StopVibration()
    {
        device.StopHaptics();
    }

    public Device()
    {
        OFF = true;
    }

    private bool findDevice()
    {

        if (OFF) return false;

        var devices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(node, devices);

        if (devices.Count > 0)
        {
            device = devices[0];
            deviceFound = true;
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool check()
    {
        if(!deviceFound)
        {
            if(!findDevice())
            {
                return false;
            }
        }
        return true;
    }

    public bool query(InputFeatureUsage<Quaternion> usage, out Quaternion value)
    {
        if (check() && device.TryGetFeatureValue(usage, out value))
        {
            return true;
        }
        else
        {
            value = Quaternion.identity;
            return false;
        }
    }

    public bool query(InputFeatureUsage<Vector3> usage, out Vector3 value)
    {
        if(check() && device.TryGetFeatureValue(usage, out value))
        {
            return true;
        }
        else
        {
            value = new Vector3();
            return false;
        }
    }

    public bool query(InputFeatureUsage<Vector2> usage, out Vector2 value)
    {
        if (check() && device.TryGetFeatureValue(usage, out value))
        {
            value.x = Mathf.Abs(value.x) < deadZone ? 0.0f : value.x;
            value.y = Mathf.Abs(value.y) < deadZone ? 0.0f : value.y;
            return true;
        }
        else
        {
            value = new Vector2();
            return false;
        }
    }

    public bool query(InputFeatureUsage<float> usage, out float value)
    {
        if (check() && device.TryGetFeatureValue(usage, out value))
        {
            return true;
        }
        else
        {
            value = 0.0f;
            return false;
        }
    }

    public bool query(InputFeatureUsage<bool> usage, out bool value)
    {
        if (check() && device.TryGetFeatureValue(usage, out value))
        {
            return true;
        }
        else
        {
            value = false;
            return false;
        }
    }

}
