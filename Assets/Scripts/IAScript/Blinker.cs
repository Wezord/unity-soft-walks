using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blinker : MonoBehaviour
{

	public float pauseTime = 0.01f;
	public float cycleTime = 1f;

    public Material material;

    void Update()
    {
		float cycleOffset = Time.time % cycleTime;

		if (cycleOffset - Time.deltaTime < pauseTime)
        {
			material.EnableKeyword("_EMISSION");
			material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
		} else
        {
			material.DisableKeyword("_EMISSION");
			material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
		}
        
       
    }
}
