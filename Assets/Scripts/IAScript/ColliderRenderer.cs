using System.Linq;
using UnityEngine;

public class ColliderRenderer : MonoBehaviour
{
    public Color color = Color.grey;

    void Start()
    {
        GetComponentsInChildren<Renderer>().ToList().ForEach(x => x.enabled = false);
        GetComponentsInChildren<Collider>().ToList().ForEach(ShowCollider);

    }

    void ShowCollider(Collider collider)
    {
        if(collider is CapsuleCollider)
        {
            var capsule = collider as CapsuleCollider;
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            Prepare(go);
			go.transform.parent = capsule.transform;
            go.transform.localPosition = capsule.center;
            var direction = capsule.direction == 0 ? Vector3.right : capsule.direction == 1 ? Vector3.up : Vector3.forward;
			go.transform.localRotation = Quaternion.FromToRotation(Vector3.up, direction);
			go.transform.localScale = new Vector3(capsule.radius * 2, capsule.height / 2, capsule.radius * 2);
        }

        if(collider is BoxCollider)
        {
			var box = collider as BoxCollider;
			var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
			Prepare(go);
			go.transform.parent = box.transform;
			go.transform.localPosition = box.center;
			go.transform.rotation = collider.transform.rotation;
			go.transform.localScale = box.size;
		}
    }

    void Prepare(GameObject go)
    {
		go.layer = gameObject.layer;
		Cleanse(go);
        go.GetComponent<Renderer>().material.color = color;
	}

    void Cleanse(GameObject go)
    {
        Remove(go.GetComponent<Collider>());
		Remove(go.GetComponent<Rigidbody>());
	}

    void Remove(Component c)
    {
        if(c != null)
        {
			Destroy(c);
		}
    }
}
