using UnityEngine;

[ExecuteAlways]
public class RadiusSetter : MonoBehaviour
{

	private BotJoint[] joints;

	public void Start()
	{
		joints = GetComponentsInChildren<BotJoint>();
	}

	public void Update()
	{
		foreach (BotJoint joint in joints)
		{
			joint.Initialize();
			var diff = joint.joint.connectedBody.transform.TransformPoint(joint.joint.connectedBody.centerOfMass) - joint.transform.TransformPoint(joint.body.centerOfMass);
			joint.baseRadius = diff.magnitude;
		}
	}


}
