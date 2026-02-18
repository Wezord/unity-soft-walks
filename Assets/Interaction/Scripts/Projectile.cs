using UnityEngine;

public class Projectile : MonoBehaviour {
    void OnCollisionEnter(Collision collider) {
        if (collider.gameObject.name == "Floor") {
            Destroy(this.gameObject, 5.0f);
        }
    }

}