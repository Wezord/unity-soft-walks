using UnityEngine;

public class Referential
{

    public Transform root;

    public Vector3 rootVelocity = Vector3.zero;

	public Referential(Transform root)
    {
        this.root = root;
    }

    public void SetRoot(Transform root)
    {
        this.root = root;
    }

    public Transform GetRoot()
    {
        return root;
    }

    public virtual Vector3 InverseTransformPoint(Vector3 point)
    {
        return root.InverseTransformPoint(point);
    }

    public virtual Vector3 TransformPoint(Vector3 point)
    {
        return root.TransformPoint(point);
    }

    public virtual Vector3 InverseTransformVector(Vector3 vec)
    {
        return root.InverseTransformDirection(vec);
    }

    public virtual Vector3 TransformVector(Vector3 vec)
    {
        return root.TransformDirection(vec);
    }

    public virtual Vector3 InverseTransformVelocity(Vector3 velocity) {
        return InverseTransformVector(velocity - rootVelocity);
    }

	public virtual Vector3 TransformVelocity(Vector3 velocity)
	{
		return TransformVector(velocity) + rootVelocity;
	}

	public virtual Vector3 InverseTransformAngularVelocity(Vector3 velocity)
	{
		return InverseTransformVector(velocity);
	}

	public virtual Vector3 TransformAngularVelocity(Vector3 velocity)
	{
		return TransformVector(velocity);
	}


	public virtual Vector3 InverseTransformNormalizedVector(Vector3 vec, float max)
    {
        return InverseTransformVector(NormalizeVector(vec, max));
    }

	public virtual Vector3 InverseTransformNormalizedPoint(Vector3 point, float max)
	{
		return InverseTransformPoint(NormalizeVector(point - root.position, max) + root.position);
	}

	public virtual Vector3 InverseTransformNormalizedVelocity(Vector3 v, Vector3 point, float baseRadius)
	{
        var radius = point.magnitude;
        var direction = point.normalized;

        v -= rootVelocity;

		var normal = Vector3.Dot(direction, v) * direction;

		var tangent = v - normal;

        tangent *= radius/baseRadius;

        v = tangent + normal;

		v += rootVelocity;

		return InverseTransformVelocity(v);
	}


	private Vector3 NormalizeVector(Vector3 v, float max)
    {
        var div = max;
        if (v.sqrMagnitude > max * max)
        {
            div = v.magnitude;
        }
        if (div == 0) return Vector3.zero;
        v /= div;
        return v;
    }

}
