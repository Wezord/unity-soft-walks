using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PID
{

    private bool first = true;

    private float lastError = 0;
    private float lastIntegral = 0.0f;

    public float integral = 0.0f;

    private Parameters param;

    public PID(Parameters param)
    {
        this.param = param;
    }

    public float get(float error, float derror)
    {

        float P = param.p1 * error;

        if (param.p3 != lastIntegral) integral = 0;
        integral += error * Time.deltaTime;
        float I = param.p3 * integral;

        float D = param.p2 * derror;

        float result = (P + I + D);

        lastError = error;
        lastIntegral = param.p3;

        return result;
    }

    public float get(float error)
    {
        if (first)
        {
            lastError = error;
            first = false;
        }
        return get(error, (error - lastError) / Time.deltaTime);
    }

}
