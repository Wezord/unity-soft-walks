using UnityEngine;
using UnityEngine.UIElements;

public class ArrowController : MonoBehaviour
{
    public GameObject prefab;
    public Transform origin;
    // if not null, the arrow will point to this target
    public Transform target;
    // if not null, the arrow will point in the direction of the speed of this rigidbody
    public Rigidbody speedBody;
    // if neither is null, the arrow will point to the forward of the origin

    public Color color = Color.white;

    public bool horizontal = false;
    public bool distance = false;
    public bool setHeight = false;

    public float height = 0;

    private GameObject arrow;
    private Transform tf;

    void Start()
    {
        arrow = Instantiate(prefab, origin.position, origin.rotation);
        tf = arrow.transform;
        arrow.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
		foreach (Renderer renderer in arrow.GetComponentsInChildren<Renderer>())
		{
			renderer.material.color = color;
		}
    }

    void Update()
    {
        Vector3 v = Vector3.up;

        tf.position = origin.position;
        if(setHeight)
        {
            tf.position = new Vector3(tf.position.x, height, tf.position.z);
        }

		if (target != null)
		{
			v = target.position - tf.position;
            if(distance)
            {
				var s = tf.localScale;
				s.z = v.magnitude;
				tf.localScale = s;
			}
        }
        else if (speedBody != null)
        {
            v = speedBody.velocity;
			if (distance)
			{
                var s = tf.localScale;
                s.z = v.magnitude / 2f;
                tf.localScale = s;
			}
		}
        else
        {
            v = origin.forward;
        }
        if(H(v).sqrMagnitude > 0)
		{
			tf.rotation = Quaternion.LookRotation(H(v));
		}
    }

    private Vector3 H(Vector3 v)
    {
        if(horizontal) v.y = 0;
        return v;
	}
}
