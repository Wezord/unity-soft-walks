using System.Collections;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.MLAgents.Policies;

public class Imitation
{

	public const float epsilon = 0.01f;
	public const float e = epsilon;

	public const float speed = 1.5f;

    [UnityTest]
    public IEnumerator RecordDemonstration()
    {
		yield return PrepareImitationScene();

		agent.SetRecording(true);

		yield return ManualImitation();
	}

	[UnityTest]
	public IEnumerator ImitationTest()
	{
		yield return PrepareImitationScene();

		yield return ManualImitation();
	}

	[UnityTest]
	public IEnumerator CollectObservationData()
	{
		yield return PrepareImitationScene();

		const int iterations = 1000;

		List<List<string>> observationData = new List<List<string>>(iterations+1);

		var targetAgent = target.GetComponent<BotAgent>();

		var header = new List<string>();
		var agentName = "Target ";
		for (int i = 0; i < 2; i++)
		{
			header.Add(agentName);
			AddHeaderVector("diff", header);
			header.Add("dying");
			foreach (var joint in agent.joints)
			{
				var name = agentName + joint.gameObject.name.Split(":").Last();
				AddHeaderVector(name + " normal", header);
				AddHeaderVector(name + " tangent", header);
				AddHeaderVector(name + " position", header);
				AddHeaderVector(name + " velocity", header);
				header.Add(name + " collision");
			}
			agentName = "Agent ";
		}

		observationData.Add(header);

		int dataCount = header.Count;

		for (int i = 0; i < iterations; i++)
		{
			CopyRootTransform(targetTransform, transform);
			CopyActions(targetTransform, agent);

			var currentList = new List<string>(dataCount);
			currentList.Add("");
			currentList.AddRange(CustomSensor.GetObservationsFromAgent(targetAgent));
			currentList.Add("");
			currentList.AddRange(CustomSensor.GetObservationsFromAgent(agent));
			observationData.Add(currentList);

			yield return null;
		}

		string path = "observations.csv";

		StreamWriter writer = new StreamWriter(path, false);
		foreach(var list in observationData)
		{
			writer.WriteLine(string.Join(",", list));
		}
		writer.Close();

	}

	GameObject obj;
	Transform transform;
	HumanBotAgent agent;
	GameObject target;
	Transform targetTransform;
	BehaviorParameters behavior;

	public IEnumerator PrepareImitationScene()
	{
		EditorSceneManager.LoadSceneInPlayMode("Assets/AI/HeidelBot/Scenes/HeidelBotGAIL.unity", new UnityEngine.SceneManagement.LoadSceneParameters());

		yield return null;

		// Get heidel bot

		obj = GameObject.Find("HeidelBot");
		Assert.IsNotNull(obj);

		transform = obj.GetComponent<Transform>();
		Assert.IsNotNull(transform);

		agent = transform.GetComponent<HumanBotAgent>();
		Assert.IsNotNull(agent);

		behavior = transform.GetComponent<BehaviorParameters>();

		agent.p.training = true;
		agent.MaxStep = 0;
		agent.IgnoreCollisions();
		behavior.BehaviorType = BehaviorType.HeuristicOnly;
		agent.p.torqueFactor = 0f;

		// Copy target human bot

		// preset to animating before instantiating copy
		agent.SetAnimating(true);

		target = GameObject.Instantiate(obj, Vector3.zero, Quaternion.identity);
		Assert.IsNotNull(target);

		//reset main agent to not animating
		agent.SetAnimating(false);

		targetTransform = target.GetComponent<Transform>();
		Assert.IsNotNull(targetTransform);

		GameObject floor = GameObject.Find("/Floor");
		floor.GetComponent<Collider>().isTrigger = true;
		//floor.layer = LayerMask.NameToLayer("NonCollide");
		Physics.gravity *= 0f;

		agent.body.isKinematic = true;
		//agent.body.constraints = RigidbodyConstraints.FreezeAll;

		foreach(var joint in agent.joints)
		{
			joint.joint.projectionMode = JointProjectionMode.PositionAndRotation;
			joint.joint.projectionDistance = 0.01f;
		}

	}

	public IEnumerator ManualImitation(int frames = 50000)
	{
		for (int i = 0; i < frames; i++)
		{
			//MoveForward();
			//CopyRootTransform(targetTransform, transform);
			//CopyTransform(targetTransform, transform);
			MoveTransform(targetTransform, transform);
			//CopyActions(targetTransform, agent);
			yield return new WaitForFixedUpdate();
		}
	}

	public void MoveForward()
	{
		transform.position += Vector3.forward * speed * Time.deltaTime;
		targetTransform.position += Vector3.forward * speed * Time.deltaTime;
	}

	public void CopyTransform(Transform source, Transform destination)
	{

		//destination.localPosition = source.localPosition;
		//destination.localRotation = source.localRotation;

		foreach (Transform tf in source)
		{
			Transform eq = destination.Find(tf.gameObject.name);
			Assert.IsNotNull(eq);
			CopyTransform(tf, eq);
		}
	}

	public void MoveTransform(Transform source, Transform destination)
	{

		var body = destination.GetComponent<Rigidbody>();
		if(body)
		{
			body.isKinematic = true;
			body.MovePosition(source.position);
			body.MoveRotation(source.rotation);
		}

		foreach (Transform tf in source)
		{
			Transform eq = destination.Find(tf.gameObject.name);
			Assert.IsNotNull(eq);
			MoveTransform(tf, eq);
		}
	}

	public void CopyRootTransform(Transform source, Transform destination)
	{

		destination.position = source.position;
		destination.localRotation = source.localRotation;

		foreach (Transform tf in source)
		{
			Transform eq = destination.Find(tf.gameObject.name);
			Assert.IsNotNull(eq);
			if(eq.TryGetComponent<BotJoint>(out var joint))
			{
				continue;
			}
			CopyRootTransform(tf, eq);
		}
	}

	public void CopyActions(Transform source, HumanBotAgent agent)
	{
		foreach(var joint in agent.joints)
		{
			Transform reference = null;
			foreach(Transform tf in source.GetComponentsInChildren<Transform>())
			{
				if(tf.gameObject.name.Equals(joint.gameObject.name))
				{
					reference = tf;
				}
			}
			Assert.IsNotNull(reference);

			CopyAction(reference, joint);

		}
	}

	public void CopyAction(Transform source, BotJoint joint)
	{
		//AssertReversibleEulerAngles(source.localEulerAngles);
		//AssertReversibleActions(joint, joint.name);

		joint.configuring = true;

		joint.SetDrive();

		Vector3 actions = joint.GetLocalRotationActions(source.localRotation);
		joint.actionX = actions.x;
		//AssertCorrectAction(actions.x, joint.name + " x");
		joint.actionY = actions.y;
		//AssertCorrectAction(actions.y, joint.name + " y");
		joint.actionZ = actions.z;
		//AssertCorrectAction(actions.z, joint.name + " z");
	}

	public void AssertCorrectAction(float action, string name)
	{
		Assert.IsTrue(-1 <= action && action <= 1, name + " action value : " + action);
	}

	public void AssertReversibleActions(BotJoint joint, string name)
	{
		Vector3 actions = joint.GetTargetRotationActions(joint.GetTargetRotation());
		UnityEngine.Assertions.Assert.AreApproximatelyEqual(actions.x, joint.actionX, e, name + " x");
		UnityEngine.Assertions.Assert.AreApproximatelyEqual(actions.y, joint.actionY, e, name + " y");
		UnityEngine.Assertions.Assert.AreApproximatelyEqual(actions.z, joint.actionZ, e, name + " z");
	}

	public void AssertReversibleEulerAngles(Vector3 eulerAngles)
	{
		Vector3 otherAngles = Quaternion.Euler(eulerAngles).eulerAngles;
		UnityEngine.Assertions.Assert.AreApproximatelyEqual(eulerAngles.x, otherAngles.x, e, "x");
		UnityEngine.Assertions.Assert.AreApproximatelyEqual(eulerAngles.y, otherAngles.y, e, "y");
		UnityEngine.Assertions.Assert.AreApproximatelyEqual(eulerAngles.z, otherAngles.z, e, "z");
	}

	public void AddHeaderVector(string name, List<string> header)
	{
		header.Add(name + " x");
		header.Add(name + " y");
		header.Add(name + " z");
	}

}
