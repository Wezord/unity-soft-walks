using UnityEngine;

public class AntiOOB : MonoBehaviour
{

    Vector3 spawnPosition;
    Rigidbody body;
    RigidbodyCopy copy;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        copy = new RigidbodyCopy(body.transform, body);
    }

    private void ResetSpawn()
    {
        copy.paste(body.transform, body);
        body.transform.Translate(Vector3.up * 2);
    }

    int downCounter = 0;
    void FixedUpdate()
    {
        if (body.velocity.y < -10)
        {
            ++downCounter;
        }
        else
        {
            downCounter = 0;
        }

        //Reset
        if (downCounter == 200)
        {
            ResetSpawn();
        }
    }
}
