using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PIDVector3
{

    public PID x, y, z;

    public Parameters param;

    public PIDVector3(Parameters parameters)
    {
        param = parameters;

        x = new PID(param);
        y = new PID(param);
        z = new PID(param);
    }

    public Vector3 get(Vector3 error)
    {

        return new Vector3(
            x.get(error.x),
            y.get(error.y),
            z.get(error.z)
            );
    }

    public Vector3 get(Vector3 error, Vector3 derror)
    {

        return new Vector3(
            x.get(error.x, derror.x),
            y.get(error.y, derror.y),
            z.get(error.z, derror.z)
            );
    }

}
