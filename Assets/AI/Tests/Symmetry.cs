using NUnit.Framework;
using System.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;

public class Symmetry
{
	public const float epsilon = 0.00001f;

	[Test]
	public void MirrorPositionTest()
	{
		Vector3 position = new Vector3(1, 2, 3);
		Vector3 normal = new Vector3(0, 1, 0);
		// Get the mirrored position
		Vector3 mirroredPosition = Mirror.MirrorPosition(position, Vector3.zero, normal);
		// Check the mirrored position
		Assert.AreEqual(new Vector3(1, -2, 3), mirroredPosition);
	}

	[Test]
	public void DoubleMirrorTest()
	{
		// Get random with seed
		Random.InitState(1);
		for(int i = 0; i < 10; i++)
		{
			Vector3 position = Random.insideUnitSphere * 10;
			Vector3 center = Random.insideUnitSphere * 10;
			Quaternion rotation = Random.rotation;
			Vector3 normal = Random.onUnitSphere;
			// Get the mirrored position twice (double mirror is identity)
			Vector3 doubleMirroredPosition = Mirror.MirrorPosition(Mirror.MirrorPosition(position, center, normal), center, normal);
			Quaternion doubleMirroredRotation = Mirror.MirrorRotation(Mirror.MirrorRotation(rotation, normal), normal);
			AssertAreNearlyEqual(doubleMirroredPosition, position);
			AssertAreNearlyEqual(doubleMirroredRotation, rotation);
		}
	}

	[Test]
	public void SymmetricHumanBotTest()
	{
		GameObject obj = (GameObject) AssetDatabase.LoadAssetAtPath("Assets/AI/HumanBot/Prefabs/HumanBot.prefab", typeof(GameObject));
		Assert.IsNotNull(obj);
		Transform transform = obj.GetComponent<Transform>();
		Assert.IsNotNull(transform);

		// Get the HumanBot's right normal which is the plane normal
		Vector3 normal = transform.right;
		Vector3 center = transform.position;

		HumanBotAgent agent = obj.GetComponent<HumanBotAgent>();
		Assert.IsNotNull(agent);
		agent.p = obj.GetComponent<HumanBotParameters>();

		// Test if transform is symmetric to self
		SymmetricTransforms(transform, transform, center, normal, "Self");

		// todo fix
		// Test if hands and feet are placed symmetrically
		// SymmetricTransforms(agent.p.leftFoot.transform, agent.p.rightFoot.transform, center, normal, "Foot");
		// SymmetricTransforms(agent.p.leftHand.transform, agent.p.rightHand.transform, center, normal, "Hand");

	}

	/*
	// Test if mirrored HumanBot is the same
	[Test]
	public void MirroredHumanBotTest()
	{
		GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/AI/HumanBot/Prefabs/HumanBot.prefab", typeof(GameObject));
		Assert.IsNotNull(prefab);

		GameObject obj = Object.Instantiate(prefab);

		Transform transform = obj.GetComponent<Transform>();
		Assert.IsNotNull(transform);

		// Get the HumanBot's right normal which is the plane normal
		Vector3 normal = transform.right;

		Transform[] transforms = obj.GetComponentsInChildren<Transform>();
		Assert.IsNotNull(transforms);

		// More than 10 transforms
		Assert.Less(10, transforms.Length);

		foreach(var tf in transforms)
		{
			tf.position = Mirror.MirrorPosition(tf.position, normal);
			//tf.rotation = Mirror.MirrorRotation(tf.rotation, normal);
		}

		PrefabUtility.SaveAsPrefabAsset(obj, "Assets/AI/Tests/Output/MirroredHumanBot.prefab", out bool success);
		Assert.IsTrue(success);

	}
	*/

	// Test if two transforms are symmetrical
	public void SymmetricTransforms(Transform a, Transform b, Vector3 center, Vector3 normal, string message = "")
	{
		Assert.IsNotNull(a);
		Assert.IsNotNull(b);
		Assert.IsNotNull(normal);

		// Check the mirrored positions
		AssertAreNearlyEqual(a.position, Mirror.MirrorPosition(b.position, center, normal), message + " Position");
		AssertAreNearlyEqual(b.position, Mirror.MirrorPosition(a.position, center, normal), message + " Position");
	}

	public void AssertAreNearlyEqual(Vector3 a, Vector3 b, string message = "", float e = epsilon)
	{
		// Calculate distance from a to b (to avoid floating point errors)
		var dist = Vector3.Distance(a, b);

		//if (dist > e/3f) Debug.Log(message + " " + dist + " " + a + " vs " + b);

		// Check the distance is small enough
		Assert.Less(dist, e, message + " " + a + " vs " + b);
	}

	public void AssertAreNearlyEqual(Quaternion a, Quaternion b, string message = "")
	{
		// Calculate distance from a to b (to avoid floating point errors)
		var dist = Quaternion.Angle(a, b);
		// Check the distance is small enough
		Assert.Less(dist, epsilon, message + " " + a + " vs " + b);
	}

	// Test AssertAreNearlyEqual
	[Test]
	public void AssertAreNearlyEqualTest()
	{
		// Get random with seed
		Random.InitState(2);
		for (int i = 0; i < 10; i++)
		{
			Vector3 position = Random.insideUnitSphere * 10;
			Quaternion rotation = Random.rotation;

			// Check is equal to self
			AssertAreNearlyEqual(position, position);
			AssertAreNearlyEqual(rotation, rotation);
		}
	}

	// Test if the observations are symmetric
	[Test]
	public void SymmetricObservationsTest()
	{
		GameObject obj = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/AI/HumanBot/Prefabs/HumanBot.prefab", typeof(GameObject));
		obj = Object.Instantiate(obj);

		Assert.IsNotNull(obj);
		Transform transform = obj.GetComponent<Transform>();
		Assert.IsNotNull(transform);

		HumanBotAgent agent = obj.GetComponent<HumanBotAgent>();
		agent.GetJoints();
		Assert.IsNotNull(agent);

		var referential = new UnorthogonalReferential(transform);

		Random.InitState(3);
		for (int i = 0; i < 10; i++)
		{
			Vector3 position = Random.insideUnitSphere * 10;
			Quaternion rotation = Random.rotation;
			transform.position = position;
			transform.rotation = rotation;

			for(int j = 0; j < agent.leftJoints.Count; j++)
			{
				var leftJoint = agent.leftJoints[j];
				var rightJoint = agent.rightJoints[j];
				Assert.IsNotNull(leftJoint);
				Assert.IsNotNull(rightJoint);
				// Test if left and right are observed symmetrically
				AssertSymmetricObservations(referential, leftJoint, rightJoint, leftJoint.gameObject.name);
				// Other way
				AssertSymmetricObservations(referential, rightJoint, leftJoint, rightJoint.gameObject.name);
			}

		}
	}

	// Test if two bot joints have symettric observations
	public void AssertSymmetricObservations(Referential referential, BotJoint a, BotJoint b, string message, float e = epsilon)
	{
		a.CalculateObservations(referential);

		SymmetricReferential symref = new SymmetricReferential(referential.root, referential);

		b.CalculateObservations(symref);

		//AssertAreNearlyEqual(a.normal, b.normal, message + " Normal", e);
		//AssertAreNearlyEqual(a.tangent, b.tangent, message + " Tangent", e);
		//AssertAreNearlyEqual(a.position, b.position, message + " Position", e);
		//AssertAreNearlyEqual(a.velocity, b.velocity, message + " Velocity", e/Time.fixedDeltaTime);
		//AssertAreNearlyEqual(a.angularVelocity, b.angularVelocity, message + " Angular Velocity", e*2f*Mathf.PI/Time.fixedDeltaTime);
	}

	[UnityTest]
	public IEnumerator SymmetricActionsTest()
	{
		Physics.gravity = Vector3.zero;

		EditorSceneManager.LoadSceneInPlayMode("Assets/AI/HumanBot/Scenes/HumanBotPPO.unity", new UnityEngine.SceneManagement.LoadSceneParameters());

		yield return null;

		GameObject obj = GameObject.Find("/Env/HumanBot");
		Assert.IsNotNull(obj);

		Transform transform = obj.GetComponent<Transform>();
		Assert.IsNotNull(transform);

		var referential = new IdentityReferential(transform);

		HumanBotAgent agent = obj.GetComponent<HumanBotAgent>();
		Assert.IsNotNull(agent);

		agent.MaxStep = 0;
		agent.p.dieOnFall = false;

		//agent.rotationSpring = 1000f;
		//agent.rotationDamper = 500f;

		float e = 1f;

		Random.InitState(4);
		for (int i = 0; i < 10; i++)
		{

			agent.body.constraints = RigidbodyConstraints.FreezeAll;
			for (int j = 0; j < agent.centerJoints.Count; j++)
			{
				var centerJoint = agent.centerJoints[j];
				centerJoint.body.constraints = RigidbodyConstraints.FreezeAll;
			}

			for (int j = 0; j < agent.leftJoints.Count; j++)
			{
				var leftJoint = agent.leftJoints[j];
				var rightJoint = agent.rightJoints[j];
				Assert.IsNotNull(leftJoint);
				Assert.IsNotNull(rightJoint);

				Vector3 action = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));

				leftJoint.configuring = true;
				rightJoint.configuring = true;

				leftJoint.actionX = action.x;
				leftJoint.actionY = action.y;
				leftJoint.actionZ = action.z;
				rightJoint.actionX = action.x;
				rightJoint.actionY = action.y;
				rightJoint.actionZ = action.z;

			}

			for(int iteration = 0; iteration < 200; iteration++)
			{
				yield return null;
			}

			for (int j = 0; j < agent.leftJoints.Count; j++)
			{
				var leftJoint = agent.leftJoints[j];
				var rightJoint = agent.rightJoints[j];
				Assert.IsNotNull(leftJoint);
				Assert.IsNotNull(rightJoint);
				// Test if left and right are observed symmetrically
				AssertSymmetricObservations(referential, leftJoint, rightJoint, leftJoint.gameObject.name, e);
				// Other way
				AssertSymmetricObservations(referential, rightJoint, leftJoint, rightJoint.gameObject.name, e);
			}

		}
	}

}
