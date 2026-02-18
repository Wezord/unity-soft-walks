using System.Collections.Generic;
using Unity.MLAgents.Policies;
using UnityEngine;

public class StiffnessGraph : MonoBehaviour
{
    public string filename = "graphs/stiffness.csv";
	public BotJoint[] joints;

	private static CSVWriter writer;

	private GaitCycle cycle;

	private List<List<string>> data = new();

	void Start()
    {
		if(writer == null) writer = new CSVWriter(filename);

		cycle = GetComponentInChildren<GaitCycle>();
	}

    void FixedUpdate()
    {
		var strs = new List<string>();
		data.Add(strs);
		strs.Add("" + GetCycleTime());
		foreach (var joint in joints)
		{
			strs.Add(CenterAngles(joint.transform.localRotation.eulerAngles).x.ToString());
		}
	}

	private float GetCycleTime()
	{
		return cycle.cycleTime;
	}


	private Vector3 GetTerrainNormal()
	{
		Vector3 position = transform.position;
		foreach(var joint in joints)
		{
			if (joint.transform.position.y < position.y)
			{
				position = joint.transform.position;
			}
		}

		if (Physics.Raycast(position, Vector3.down, out RaycastHit hit, float.MaxValue, ~(1<<gameObject.layer)))
		{
			return hit.normal;
		}
		return Vector3.up;
	}

	private Vector3 CenterAngles(Vector3 angles)
	{
		return new Vector3(
						-Mathf.DeltaAngle(0, angles.x),
									-Mathf.DeltaAngle(0, angles.y),
												-Mathf.DeltaAngle(0, angles.z)
														);
	}

	private static bool hasStarted = false;
	private bool hasWritten = false;

	private static List<List<List<string>>> final_data = new();


	private void Write()
	{
		if (!hasStarted)
		{
			hasStarted = true;

			writer.Add("Gait Cycle Time");
			
		}

		

		if (!hasWritten)
		{
			hasWritten = true;

			foreach (var joint in joints)
			{
				writer.Add(name + " " + joint.name + " rotation");
			}

			final_data.Add(data);
		}
	}


	private void OnApplicationQuit()
	{
		if (writer == null) return;
		Write();
	}

	private void OnDestroy()
	{
		if(writer == null) return;

		// Finish header
		writer.NextLine();

		int tabs = 0;

		foreach(var agent in final_data)
		{

			foreach(var line in agent)
			{
				int index = 0;
				foreach(var item in line)
				{
					index++;
					if (index == 1)
                    {
						writer.Add(item);

						for (int i = 0; i < tabs; i++)
						{
							writer.Add("");
						}

						continue;
					}

					writer.Add(item);
				}
				writer.NextLine();
			}
			writer.NextLine();

			tabs += agent[0].Count - 1;
		}

		writer.Write();

		writer = null;
	}
}
