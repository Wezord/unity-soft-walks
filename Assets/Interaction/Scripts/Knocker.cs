using UnityEngine;

public class Knocker : MonoBehaviour
{
    public float knockback, upAmplitude;
    
    public Rigidbody target;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.rigidbody == target)
        {
            var impulse = -collision.relativeVelocity * knockback;
            impulse.y = Mathf.Abs(collision.relativeVelocity.magnitude * upAmplitude);
            target.AddForce(impulse, ForceMode.Impulse);
        }
    }
}
