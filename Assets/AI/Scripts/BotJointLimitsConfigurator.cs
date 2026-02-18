using System;
using UnityEngine;

[ExecuteAlways]
public class BotJointLimitsConfigurator : MonoBehaviour
{
	public bool configureJoints = false;

	private bool isInitialized = false;

	private ConfigurableJoint joint;

	// struct to represent the rotations
	private struct RotationAngles{
		public float xLow;
		public float xHigh;
		public float y;
		public float z;
	}
	private RotationAngles rotationAnglesLimits;

	private void initRotationAngles(){
		rotationAnglesLimits.xLow = joint.lowAngularXLimit.limit;
		rotationAnglesLimits.xHigh = joint.highAngularXLimit.limit;
		rotationAnglesLimits.y = joint.angularYLimit.limit;
		rotationAnglesLimits.z = joint.angularZLimit.limit;
	}

	private void displayRotationAngles(){
		Debug.Log(joint.name + "\n\t"
					+ " x low: " + rotationAnglesLimits.xLow
					+ ", x high: " + rotationAnglesLimits.xHigh
					+ ", y: " + rotationAnglesLimits.y
					+ ", z: " + rotationAnglesLimits.z
		);
	}

	public void OnApplicationQuit(){
		if(configureJoints)
			displayRotationAngles();
	}

	private void Start()
	{
		Initialize();
	}

	public void Initialize()
    {
        if (isInitialized) return;
		joint = GetComponent<ConfigurableJoint>();
		initRotationAngles();
		isInitialized = true;
	}

	public void UpdateLimits(Vector3 rotations, Vector3 actions){
		if (!AssertCorrectAction(actions.x, rotations.x, joint.name + " x")){
			if(actions.x < -1){
				if(rotations.x < rotationAnglesLimits.xLow)
					rotationAnglesLimits.xLow = rotations.x;
			}
			if(actions.x > 1){
				if(rotations.x > rotationAnglesLimits.xHigh)
					rotationAnglesLimits.xHigh = rotations.x;
			}
		}
		if(!AssertCorrectAction(actions.y, rotations.y, joint.name + " y")){
			if(Math.Abs(rotations.y) > rotationAnglesLimits.y)
				rotationAnglesLimits.y = Math.Abs(rotations.y);
		}
		if(!AssertCorrectAction(actions.z, rotations.z, joint.name + " z")){
			if(Math.Abs(rotations.z) > rotationAnglesLimits.z)
				rotationAnglesLimits.z = Math.Abs(rotations.z);
		}

	}

	public bool AssertCorrectAction(float action, float rotation, string name)
	{
		if (!(-1 < action && action < 1))
		{
			Debug.Log(name + " action value : " + action + " rotation angle : " + rotation);
			// Debug.Break();
			return false;
		}
		return true;
	}

}
