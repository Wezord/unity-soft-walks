using UnityEngine;

public class Restarter : MonoBehaviour
{

    private Vector3 position;
    private Quaternion rotation;

    private void Start()
    {
        position = transform.position;
        rotation = transform.rotation;
    }

    public void Restart()
    {
		transform.position = position;
		transform.rotation = rotation;
	}

    public void SetStart()
    {
        Start();
    }

}
