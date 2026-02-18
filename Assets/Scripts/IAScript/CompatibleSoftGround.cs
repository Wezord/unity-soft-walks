using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using System.IO;

public class CompatibleSoftGround : MonoBehaviour
{

	public GameEventFloat maxDepthChangeEvent;

	public float beta;
	public float C;
	public float C2;
	public float g;
	public float g2;
	public float unitWeight;
	public float height;
	public Transform soilTop;
	public float nu;

	public float friction;
	public float angularFriction;

	public float maxDepth;
	public float jointDepth;
	public float limitDepth;
	public float baseLimit;

	public float limitSpring, limitDamper, limitContactDistance;

	private float gamma;

	private List<Collider> isColliding = new List<Collider>();

	public int nbDt = 1;
	private float dt;

	public int nbCell = 5;

	private Rigidbody body;

    public GameEventCollision collisionSoft;

	private Vector3 contactPoint;

	private String path;

	public struct Face
	{
		public Vector3 norm;
		public Vector3 center;
		public Vector2 dim;
		public List<Vector3> base2D;
	}

	void Start()
	{
		gamma = g; // * C;

		dt = Time.fixedDeltaTime;
		//nbDt = Mathf.FloorToInt(1000 * dt) + 1;

		body = GetComponent<Rigidbody>();

		maxDepthChangeEvent.RegisterListener((float newDepth)=>
		{
			maxDepth = newDepth;
		});

		path = Application.dataPath + "/export.csv";
	}

	/*void FixedUpdate()
	{

		for (int i = 0; i < isColliding.Count; i++)
		{
			var collider = isColliding[i];

			if(collider == null) {
				Debug.Log("pas de collider");
				continue;
			}

			Rigidbody part = collider.attachedRigidbody;
			float groundOrigin = Terrain.activeTerrain.SampleHeight(collider.transform.position);

			List<Face> listF = computeFaces(collider);

			float maxEps = 0;

			for (int k = 0; k < 3; k++)
			{
				Vector3[,] points;
				float dS;
				(points, dS) = Pressure(listF[k], nbCell);

				if (Mathf.Abs(Vector3.Dot(listF[k].norm, new Vector3(0f, 1f, 0f))) > 0.05f)
				{
					foreach (Vector3 point in points)
					{

						float realEps = groundOrigin - point.y;

						Vector3 norm = listF[k].norm;

						Vector3 finalPoint = point;

						if (collider is CapsuleCollider)
						{
							CapsuleCollider coll = (CapsuleCollider)collider;

							(Vector3 dim, Vector3 ctr) = dimensionsCollider(coll);

							float r = dim.x;
							float h = dim.y;

							if (listF[k].dim.x == h || listF[k].dim.y == h)
							{

								Vector3 origine = coll.transform.position + Vector3.Dot(point - coll.transform.position, coll.transform.up) * coll.transform.up;

								Vector3 dir = point - origine;

								norm = dir.normalized;

								finalPoint = origine + r * dir.normalized;

								realEps = groundOrigin - finalPoint.y;
							}
						}

						maxEps = Mathf.Max(realEps, maxEps);

						Debug.Log(realEps);
						if (realEps > 0)
						{
							Vector3 proj = Vector3.Dot(norm, Vector3.up) * Vector3.up;

							//EULER
							float e = realEps;
							float v = part.GetPointVelocity(finalPoint).y;

							var F =  dS * StressStrain(e, v);

							maxDepthChangeEvent.Raise(-e);

							Debug.Log("d");

							part.AddForceAtPosition(-F * proj, finalPoint);
						}
					}
				}
			}
		}

	}*/
	float[] valueGraph = new float[3];
    void OnCollisionStay(Collision collision){
		Rigidbody part = collision.rigidbody;
		float groundOrigin = Terrain.activeTerrain.SampleHeight(collision.transform.position);

		Collider collider = collision.collider;

		List<Face> listF = computeFaces(collider);

		// Trouver la face la plus proche du sol
		int closestFaceIndex = 0;
		float minY = float.MaxValue;
		
			for (int k = 0; k < listF.Count; k++)
			{
				if(listF[k].center.y < minY && Vector3.Dot(listF[k].norm, Vector3.down) > 0.7f)
				{
					minY = listF[k].center.y;
					closestFaceIndex = k;
				}
			}


		float maxEps = 0f;
		float maxF = 0f;
		float realEps = 0f;
		for (int k = 0; k < 3; k++)
			{
				float currentF = 0f;
				Vector3[,] points;
				float dS;
				float[] predictedEList = new float[nbCell * nbCell];
				(points, dS) = Pressure(listF[k], nbCell);
				if(k == closestFaceIndex)
					valueGraph[0] = part.position.y;
				if (Mathf.Abs(Vector3.Dot(listF[k].norm, new Vector3(0f, 1f, 0f))) > 0.05f)
				{
					float moyenne_depth = 0f;
					float[] depths = new float[points.GetLength(0) * points.GetLength(1)];
					int depth_index = 0;
					foreach (Vector3 point in points)
					{

						realEps = groundOrigin -0.01f - point.y;

						//Debug.Log("realEps init " + realEps + " " + groundOrigin);

						Vector3 norm = listF[k].norm;

						Vector3 finalPoint = point;

						if (collider is CapsuleCollider)
						{
							CapsuleCollider coll = (CapsuleCollider)collider;

							(Vector3 dim, Vector3 ctr) = dimensionsCollider(coll);

							float r = dim.x;
							float h = dim.y;

							if (listF[k].dim.x == h || listF[k].dim.y == h)
							{

								Vector3 origine = coll.transform.position + Vector3.Dot(point - coll.transform.position, coll.transform.up) * coll.transform.up;

								Vector3 dir = point - origine;

								norm = dir.normalized;

								finalPoint = origine + r * dir.normalized;

								realEps = groundOrigin - finalPoint.y;
							}
						}
						if (valueGraph[0] != 0 && valueGraph[1] != 0 && collision.gameObject.name == "mixamorig:LeftFoot"  && k == closestFaceIndex){
							valueGraph[2] = part.position.y;

							string path = Application.dataPath + "/Exports/EulerSimulationLeftFoot.csv";

							using (StreamWriter writer = new StreamWriter(path, true))
							{
								writer.WriteLine(Time.time.ToString("F4") + "," + valueGraph[0] + "," + valueGraph[1] + "," + valueGraph[2]);
							}

							valueGraph = new float[3];

						}
						else if (valueGraph[0] != 0 && valueGraph[1] != 0 && collision.gameObject.name == "mixamorig:RightFoot" &&  k == closestFaceIndex){
							valueGraph[2] = part.position.y;

							string path = Application.dataPath + "/Exports/EulerSimulationRightFoot.csv";

							using (StreamWriter writer = new StreamWriter(path, true))
							{
								writer.WriteLine(Time.time.ToString("F4") + "," + valueGraph[0] + "," + valueGraph[1] + "," + valueGraph[2]);
							}

							valueGraph = new float[3];

						}

						valueGraph[0] = realEps;

						maxEps = Mathf.Max(realEps, maxEps);

						if (realEps > 0)
						{
							Vector3 proj = Vector3.Dot(norm, Vector3.up) * Vector3.up;

							//EULER
							float e = realEps;
							float v = part.GetPointVelocity(finalPoint).y;


							//Debug.Log("realEps: " + realEps + " velocityY: " + v);

							var (F, predictedE) =  EulerSimulation(e, v, dS, part.mass);
							depths[depth_index] = -predictedE;
							depth_index += 1;
							part.AddForceAtPosition(-F * proj, finalPoint);


							currentF +=F;

						}

					}
					moyenne_depth = depths.Sum() / depths.Length;
					maxDepthChangeEvent.Raise(depths.Max());
					if(k == closestFaceIndex)
					{
						(Vector3 size, Vector3 center) = dimensionsCollider(collider);
						valueGraph[1] = depths.Max() + size.y / 2f - groundOrigin;
						maxF = Mathf.Max(currentF, maxF);
					}
				}
			}
			float jointForce = part.gameObject.GetComponent<Joint>().currentForce.y;
			//Debug.Log("maxF: " + maxF + " collision impulse: " + collision.impulse.y / dt);
			//Debug.Log("maxF: " + maxF + " collision impulse: " + jointForce);
			/*if(maxF < -jointForce  && maxF > 0f){
				// Get vertical velocity at the contact point
				float velocityY = part.GetPointVelocity(collision.GetContact(0).point).y;

				// Calculate depth considering velocity and impulse
				float depth = velocityY * dt + 0.5f * (-jointForce - maxF) * dt * dt / 70f;
				valueGraph[1] = depth;

				// Debug log the depth and forces
				//Debug.Log($"VelocityY: {velocityY}, Impulse: {collision.impulse.y / dt}, Max Force: {maxF}, Depth: {depth}");

				// Raise event with the calculated depth
				maxDepthChangeEvent.Raise(depth);

				collisionSoft.Raise(collision);
			}
			else {
				maxDepthChangeEvent.Raise(0f);
			}*/
	}

	public void ModificationEvent(PhysicsScene scene, NativeArray<ModifiableContactPair> pairs)
	{

		foreach (ModifiableContactPair pair in pairs)
		{
			for (int i = 0; i < pair.contactCount; i++)
			{
				//pair.IgnoreContact(i);

				/*Collider colA = pair.

				Rigidbody part = pair.rigidbody;

				float groundOrigin = Terrain.activeTerrain.SampleHeight(collision.transform.position);

				Collider collider = collision.collider;

				List<Face> listF = computeFaces(collider);

				Vector3[,] points;
				float dS;
				(points, dS) = Pressure(listF[k], nbCell);

				var sep = pair.GetSeparation(i);

				float e = pair.GetPoint(i).y;

				var F =  dS * StressStrain(e, pair.GetTargetVelocity(i).y);*/

				//pair.SetSeparation(i, Mathf.Max(sep + maxDepth, 0));

				contactPoint = pair.GetPoint(i);
			}
		}
	}

	private (Vector3[,], float) Pressure(Face face, int nbCell)
	{
		Vector3[,] sample = new Vector3[nbCell, nbCell];

		Vector2 dim = face.dim;

		float dx = dim.x / nbCell;
		float dy = dim.y / nbCell;

		Vector3 corner = face.center - 0.5f * dim.x * face.base2D[0] - 0.5f * dim.y * face.base2D[1];

		for (int i = 0; i < nbCell; i++)
		{
			for (int j = 0; j < nbCell; j++)
			{
				sample[i, j] = corner + dx * i * face.base2D[0] + dy * j * face.base2D[1];

			}
		}

		return (sample, dx * dy);

	}

	public float StressStrain(float eps, float v)
	{
		if (eps > 0)
		{
			if (v <= 0)
			{
				return C * Mathf.Pow(eps, beta) - gamma * v + unitWeight * height;
			}
			else
			{
				return C2 * Mathf.Pow(eps, beta) - g2 * v + unitWeight * height;
			}
		}

		else
		{
			return 0f;
		}


	}

	private (float, float) EulerSimulation(float e, float v, float dS, float mass)
	{
		//EULER
		float F = 0f;
		float ddt = dt / nbDt;

		for (int j = 0; j < nbDt; j++)
		{
			float f = dS * StressStrain(e, v);
			F = F + f / nbDt;

			e = e - v * ddt;
			v = v + f * ddt / mass;
		}

		return (F, e);
	}

	private (Vector3, Vector3) dimensionsCollider(Collider collider)
	{
		Vector3 size = new Vector3(0f, 0f, 0f);
		Vector3 center = new Vector3(0f, 0f, 0f);


		if (collider.gameObject.GetComponent<BoxCollider>())
		{
			BoxCollider Bcoll = collider.gameObject.GetComponent<BoxCollider>();

			size.x = GetGlobalSize(Vector3.right * Bcoll.size.x, Bcoll.transform);
			size.y = GetGlobalSize(Vector3.up * Bcoll.size.y, Bcoll.transform);
			size.z = GetGlobalSize(Vector3.forward * Bcoll.size.z, Bcoll.transform);

			center = Bcoll.center;
		}

		if (collider.gameObject.GetComponent<CapsuleCollider>())
		{
			CapsuleCollider Ccoll = collider.gameObject.GetComponent<CapsuleCollider>();

			size.x = GetGlobalSize(Vector3.right * Ccoll.radius, Ccoll.transform);
			size.y = GetGlobalSize(Vector3.up * Ccoll.height, Ccoll.transform);
			size.z = GetGlobalSize(Vector3.forward * Ccoll.radius, Ccoll.transform);

			center = Ccoll.center;
		}

		return (size, center);
	}

	private List<Face> computeFaces(Collider collider)
	{
		Face face1 = new Face();
		Face face2 = new Face();
		Face face3 = new Face();

		Vector3 norm1 = Mathf.Sign(Vector3.Dot(collider.transform.right, new Vector3(0f, -1f, 0f))) * collider.transform.right; //x
		Vector3 norm2 = Mathf.Sign(Vector3.Dot(collider.transform.up, new Vector3(0f, -1f, 0f))) * collider.transform.up; //y
		Vector3 norm3 = Mathf.Sign(Vector3.Dot(collider.transform.forward, new Vector3(0f, -1f, 0f))) * collider.transform.forward; //z

		face1.norm = norm1.normalized;
		face2.norm = norm2.normalized;
		face3.norm = norm3.normalized;

		(Vector3 size, Vector3 center) = dimensionsCollider(collider);

		face1.dim = new Vector2(size.y, size.z);
		face2.dim = new Vector2(size.z, size.x);
		face3.dim = new Vector2(size.x, size.y);

		face1.center = collider.transform.position + center + 0.5f * size.x * norm1;
		face2.center = collider.transform.position + center + 0.5f * size.y * norm2;
		face3.center = collider.transform.position + center + 0.5f * size.z * norm3;


		face1.base2D = new List<Vector3> { norm2, norm3 };
		face2.base2D = new List<Vector3> { norm3, norm1 };
		face3.base2D = new List<Vector3> { norm1, norm2 };


		List<Face> list = new List<Face> { face1, face2, face3 };

		return list;
	}

	private float GetGlobalSize(Vector3 localSize, Transform tf)
	{
		float v = tf.TransformVector(localSize).magnitude;
		return v;
	}

	public void OnEnable()
	{
		GetComponent<Collider>().hasModifiableContacts = true;
        Physics.ContactModifyEvent += ModificationEvent;
    }

	public void OnDisable()
	{
		Physics.ContactModifyEvent -= ModificationEvent;
	}

}
