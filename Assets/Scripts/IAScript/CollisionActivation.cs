using UnityEngine;

public class CollisionActivation : MonoBehaviour
{

    private Rigidbody body = null;

    private void Start()
    {
        /*
        if (!Application.isPlaying)
        {
            var renderer = transform.GetComponent<MeshRenderer>();

            if (renderer != null)
            {
                renderer.sharedMaterial = (Material)AssetDatabase.LoadAssetAtPath("Assets/Materials/LocalSpace.mat", typeof(Material));
            }
        }
        */
    }

    private void OnCollisionEnter()
    {
        if(body == null) body = gameObject.AddComponent<Rigidbody>();
    }
}
