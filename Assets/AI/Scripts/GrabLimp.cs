using System.Collections.Generic;
using UnityEngine;

public class GrabLimp : MonoBehaviour
{

	public float maxForceFactor = 0.00001f;

	private List<ConfigurableJoint> joints;

	private static bool staticInitialized = false;

	void Awake()
    {
		StaticInitialize();
	}

	private static void StaticInitialize()
	{
		if (staticInitialized) return;

		Events.grabEvent += OnGrab;
		Events.ungrabEvent += OnUnGrab;

		staticInitialized = true;
	}

	private static void OnGrab(GameObject grabber, GameObject obj, PlayerType type)
    {
		var limp = Find(obj);
		if(limp != null) limp.GoLimp();
	}
	private static void OnUnGrab(GameObject grabber, GameObject obj, PlayerType type)
	{
		var limp = Find(obj);
		if (limp != null) limp.GoLimp(false);
	}

	private static GrabLimp Find(GameObject obj)
	{

		if (obj == null) return null;
		
		GameObject o = obj;
		GrabLimp limp;
		List<ConfigurableJoint> joints = new List<ConfigurableJoint>();

		do
		{
			ConfigurableJoint joint = o.GetComponent<ConfigurableJoint>();
			if (joint == null) return null;
			joints.Add(joint);

			if (joint.connectedBody == null) return null;

			o = joint.connectedBody.gameObject;

			limp = o.GetComponent<GrabLimp>();
		} while (limp == null);

		limp.joints = joints;
		return limp;
	}

	private void GoLimp(bool limp = true)
	{
		foreach(var joint in joints)
		{
			var drive = joint.slerpDrive;
			drive.maximumForce *= limp ? maxForceFactor : 1f/maxForceFactor; 
			joint.slerpDrive = drive;
		} 
	}
}
