using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioBat : MonoBehaviour
{

    public AudioSource sfx;
    public float forceVolumeFactor;
    public float minimumForceSound;

    private void OnCollisionEnter(Collision collision)
    {
        if (sfx.isPlaying) return;

        float mass = 1f;
        if (collision.rigidbody) mass = collision.rigidbody.mass;
        float mag = mass * collision.relativeVelocity.magnitude / Time.fixedDeltaTime;
        if (mag > minimumForceSound)
        {
            sfx.volume = mag * forceVolumeFactor;
            sfx.Play();
        }
    }

}
