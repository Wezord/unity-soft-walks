using System;
using System.Runtime.InteropServices;

using UnityEngine;

class Utils
{
    private static int? _vrLayer;
    public static int VRLayer
	{
        get {
            if(_vrLayer is int vrLayer)
			{
                return vrLayer;
			} else
			{
                var ret = LayerMask.NameToLayer("VRPlayer");
                _vrLayer = ret;
                return ret;
            }
        }
	}
    
    private static int? _fpsLayer;
    public static int FPSLayer
    {
        get
        {
            if (_fpsLayer is int fpsLayer)
            {
                return fpsLayer;
            }
            else
            {
                var ret = LayerMask.NameToLayer("FPSPlayer");
                _fpsLayer = ret;
                return ret;
            }
        }
    }

    public static bool IsLayerType(int layer, PlayerType type)
	{
        switch(type)
		{
            case PlayerType.VR:
                return layer == VRLayer;
            
            default:
                return layer == FPSLayer;
        }
	}

    
    public static Vector3 ModVec3(Vector3 v, [Optional] float? x, [Optional] float? y, [Optional] float? z)
    {
        if (x is float vx) v.x = vx;
        if (y is float vy) v.y = vy;
        if (z is float vz) v.z = vz;
        return v;
    }

    public static float sq(float a)
    {
        return a * a;
    }

    public static float sdiv(float a, float b)
    {
        if (b == 0.0f) return 1.0f;
        return a / b;
    }

    public static Vector3 vdiv(Vector3 a, Vector3 b)
    {
        return new Vector3(sdiv(a.x, b.x), sdiv(a.y, b.y), sdiv(a.z, b.z));
    }

    public static Vector3 vmul(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }

    public static Vector3 vsq(Vector3 a)
    {
        return new Vector3(a.x * a.x, a.y * a.y, a.z * a.z);
    }
    public static Vector3 vssq(Vector3 a)
    {
        return new Vector3(Math.Abs(a.x) * a.x, Math.Abs(a.y) * a.y, Math.Abs(a.z) * a.z);
    }

    public static float smov(float current, float target, float step)
    {
        return current > target ? target : Mathf.Min(current + step, target);
    }

    public static Vector3 vmov(Vector3 current, Vector3 target, float step)
    {
        return new Vector3(smov(current.x, target.x, step), smov(current.y, target.y, step), smov(current.z, target.z, step));
    }

    public static string vstr(Vector3 a)
    {
        return a.x + " " + a.y + " " + a.z;
    }

    public static Vector3 vclamp(Vector3 a, float min, float max)
    {
        return new Vector3(Math.Clamp(a.x, min, max), Math.Clamp(a.y, min, max), Math.Clamp(a.z, min, max));
    }

    public static Vector3 vmax(Vector3 a, float max)
    {
        return new Vector3(Math.Max(a.x, max), Math.Max(a.y, max), Math.Max(a.z, max));
    }

    public static Vector3 vmin(Vector3 a, float min)
    {
        return new Vector3(Math.Min(a.x, min), Math.Min(a.y, min), Math.Min(a.z, min));
    }

    public static bool IsFinite(Vector3 a) {
        if(float.IsFinite(a.x) && float.IsFinite(a.y) && float.IsFinite(a.z))
        {
            return true;
        }
        return false;
    }

    public static float NormalizeAngle(float theta)
    {
        float norm_theta = theta % 360f;
        if (norm_theta > 180f)
        {
            norm_theta = -360f + norm_theta;
        }
        else if (norm_theta < -180f)
        {
            norm_theta = 360f + norm_theta;
        }

        return norm_theta;
    }

}
