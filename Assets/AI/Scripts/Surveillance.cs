using System;
using UnityEngine;
using static Events;

public class Surveillance : MonoBehaviour
{

	public new Camera camera;
	public float soundRange = 5f, visionRange = 30, pointSize = 1;

	public event CrimeEvent witnessEvent;

	// Can optimise raycast by using a layer mask
	// Can optimise raycast by using a delay or queue into thread
	private void OnCrime(Vector3 position, GameObject crimer, GameObject crimee, CrimeType type, int level, float sound) {

		float distance = Vector3.Distance(camera.transform.position, position);
		bool inSoundRange = distance < soundRange;
		bool inVisionRange = distance < visionRange;
		
		// if can only see
		if(!inSoundRange && inVisionRange)
		{
			// test if in vision
			Collider[] colliders = crimer.GetComponentsInChildren<Collider>();
			Bounds bounds = new Bounds(position, Vector3.one * pointSize);
			foreach (Collider c in colliders)
			{
				bounds.Encapsulate(c.bounds);
			}

			Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
			if (!GeometryUtility.TestPlanesAABB(planes, bounds))
			{
				inVisionRange = false;
			}
		}

		// if can't detect
		if (!inSoundRange && !inVisionRange) return;

		int layermask = LayerMask.GetMask(LayerMask.LayerToName(crimer.layer), LayerMask.LayerToName(gameObject.layer));

		// test if object is in the way
		if(!Physics.Raycast(position, camera.transform.position - position, distance, ~layermask))
		{
			WitnessCrime(position, crimer, crimee, type, level, sound);
		}
	}

	public void WitnessCrime(Vector3 position, GameObject crimer, GameObject crimee, CrimeType type, int level, float sound)
	{
		// go into attack mode
		if(witnessEvent != null) witnessEvent(position, crimer, crimee, type, level, sound);
	}

	private void OnEnable()
	{
		crimeEvent += OnCrime;
	}
	private void OnDisable()
	{
		crimeEvent -= OnCrime;
	}
}
