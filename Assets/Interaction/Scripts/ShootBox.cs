using UnityEngine;

public class ShootBox : MonoBehaviour {
    public GameObject mProjectile;
    public float mLaunchForce = 2000.0f;
    public float mProjectileMass = 10.0f;

    void Update() {
        if (Input.GetKeyDown(KeyCode.X)) {
            var newPosition = transform.position + transform.GetComponent<Rigidbody>().velocity*Time.fixedDeltaTime*10;
            Vector3 randomPosition = new Vector3(
                Random.Range(newPosition.x-3, newPosition.x+3),
                Random.Range(newPosition.y, newPosition.y+2),
                Random.Range(newPosition.z-3, newPosition.z+3)
            );
            GameObject projectile = Instantiate<GameObject>(mProjectile, randomPosition, transform.rotation);
            projectile.transform.localScale = Random.Range(0.1f, 1.0f) * Vector3.one;
            projectile.GetComponent<Rigidbody>().mass = mProjectileMass;
            projectile.GetComponent<Rigidbody>().AddForce((newPosition - randomPosition).normalized * mLaunchForce);
        }
    } 

}