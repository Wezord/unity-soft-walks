using System.Collections;
using Unity.MLAgents.Policies;
using UnityEngine;

public class Stats : MonoBehaviour
{
	public float timeScale = 1f;

	// seconds
	public float testDuration = 60f;

	public double averageSurvivalTime = 0;

	public int fallCount = 0;

	public double averageSpeed = 0;

	public string filename;

	private double startTime;

	private double countEpisodes = 0;
	private double sumSurvivalTime = 0;
	private double beginTime;

	private double countFrames = 0;
	private double sumSpeed = 0;

	private double sumEnergy = 0;
	private double averageEnergy = 0;

	private Rigidbody body;
	private HumanBotParameters p;

	private static CSVWriter writer;

	void Awake()
    {
		if (!enabled) return;

		if (writer == null) writer = new CSVWriter(filename);

		Random.InitState(47);

		Time.timeScale = timeScale;

		p = GetComponent<HumanBotParameters>();
		body = GetComponent<Rigidbody>();

		//p.maxFrames = 0;
		p.randomizing = false;

        AgentEvents events = GetComponent<AgentEvents>();
		events.beginEpisodeEvent += OnEpisodeBegin;
		events.endEpisodeEvent += OnEpisodeEnd;

		startTime = Time.fixedTime;
	}

	private void OnEpisodeBegin()
	{
		beginTime = Time.timeAsDouble;
	}

	private void OnEpisodeEnd()
	{
		sumSurvivalTime += (Time.timeAsDouble - beginTime);
		countEpisodes++;
		averageSurvivalTime = sumSurvivalTime / countEpisodes;

		fallCount++;
	}

	private void FixedUpdate()
	{
		countFrames++;

		sumSpeed += BotAgent.Horizontal(body.velocity).magnitude;
		averageSpeed = sumSpeed / countFrames;

		sumEnergy += ((HumanBotAgent)p.agent).GetConsumedEnergy();
		averageEnergy = sumEnergy / countFrames;

		if (Time.fixedTime > startTime + testDuration)
		{
			Write();
			/*
			print("Stats : ");
			print("Survival time : ");
			print(averageSurvivalTime);
			print("Fall Count : ");
			print(fallCount);
			print("Average speed : ");
			print(averageSpeed);
			*/
			Debug.Break();
		}
	}

	private static bool hasStarted = false;
	private bool hasWritten = false;

	private void Write()
	{
		if(!hasStarted)
		{
			hasStarted = true;

			writer.Add("Name");

			writer.Add("Model");

			writer.Add("Average speed");

			writer.Add("Average Gait Cycle");

			writer.Add("Falls per minute");

			writer.Add("Energy Usage");


			writer.NextLine();
		}

		if(!hasWritten)
		{
			hasWritten = true;

			writer.Add(name);
			writer.Add(p.GetComponent<BehaviorParameters>().Model.name);
			writer.Add(averageSpeed.ToString());
			writer.Add(GetComponentInChildren<GaitCycle>().averageCycle.ToString());
			writer.Add((fallCount / (countFrames * Time.fixedDeltaTime / 60)).ToString());
			writer.Add(averageEnergy.ToString());

			writer.NextLine();
		}
	}


	private void OnApplicationQuit()
	{
		if (writer == null) return;
		writer.Write();
	}
}
