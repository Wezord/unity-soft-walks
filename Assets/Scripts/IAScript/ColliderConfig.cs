using UnityEngine;

[ExecuteAlways]
public class ColliderConfig : MonoBehaviour
{
    public Transform start, end;

    private Collider col;
    private CapsuleCollider cap;
    private BoxCollider box;

    void Start()
    {
        col = GetComponent<Collider>();
        if(col)
        {
            if(col is CapsuleCollider)
            {
                cap = (CapsuleCollider)col;
            }
            if(col is BoxCollider)
            {
                box = (BoxCollider)col;
            }
        }
    }

    void Update()
    {
        if (!IsActive()) return;

        if(cap)
        {
            cap.center = cap.transform.InverseTransformPoint((start.position + end.position) / 2f);
            cap.height = Vector3.Distance(start.position, end.position) + cap.radius*2;
		}

        if (box)
        {

        }

    }

	public bool IsActive()
	{
		return col && end && start && enabled && !Application.isPlaying;
	}
}
