using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trace : MonoBehaviour
{
    public GameObject target;
    public float interval = 1;

    private float lastInterval = 0;

    void Start()
    {
        
    }

    void FixedUpdate()
    {
        // Check if the interval has passed
        if (Time.fixedTime - lastInterval >= interval)
        {
			// Reset the interval
			lastInterval = Time.fixedTime;

            // Create copy of the target and freeze it
            GameObject copy = Instantiate(target, target.transform.position, target.transform.rotation);

			var childTrace = copy.GetComponent<Trace>();
			if (childTrace) Destroy(childTrace);

            var rbs = copy.GetComponentsInChildren<Rigidbody>();
			foreach(var rb in rbs)
			{
				rb.isKinematic = true;
				// Freeze constraints for all dof
				rb.constraints = RigidbodyConstraints.FreezeAll;
			}

			var anims = copy.GetComponentsInChildren<Animator>();
			foreach (var anim in anims)
			{
				Destroy(anim);
			}
		}
	}
}
