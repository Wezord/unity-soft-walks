using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class CollisionDetectorGenerator : MonoBehaviour{

    [NonSerialized]
    public HumanBotAgent agent;

    void Start(){
        agent = GetComponent<HumanBotAgent>();
        Assert.IsNotNull(agent);
        GenerateCollisionDetectors(transform);
    }

    void GenerateCollisionDetectors(Transform bone)
    {
        var joints = transform.GetComponentsInChildren<BotJoint>();
        
		// get list of feet and their parent joints
		List<BotJoint> legJoints = new List<BotJoint>();
		foreach(var foot in agent.p.feet)
		{
			legJoints.Add(foot);
			var parent = foot.transform.parent;
			while (parent != null)
			{
				var jointParent = parent.GetComponent<BotJoint>();
				if (jointParent)
				{
					legJoints.Add(jointParent);
					break;
				}
				parent = parent.parent;
			}
		}
		
	    foreach (var joint in joints)
		{
            // check if not foot or not parent of foot
            if(!legJoints.Contains(joint)){
                joint.gameObject.AddComponent<CollisionDetector>().Init(agent);
            }
		}
        gameObject.AddComponent<CollisionDetector>().Init(agent);
	}

}