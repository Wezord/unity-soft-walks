using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;

public class Recording
{
    private int index;
	private List<string> lines = new List<string>();

	private Transform player, leftHand, rightHand;
	private Rigidbody playerBody, leftBody, rightBody;
	private Grabber leftGrabber, rightGrabber;

	public int seed = 0;

	public Recording(Transform player, Transform leftHand, Transform rightHand)
	{
		this.player = player;
		this.leftHand = leftHand;
		this.rightHand = rightHand;
		playerBody = player.GetComponent<Rigidbody>();
		leftBody = leftHand.GetComponent<Rigidbody>();
		rightBody = rightHand.GetComponent<Rigidbody>();
		leftGrabber = leftHand.GetComponent<Grabber>();
		rightGrabber = rightHand.GetComponent<Grabber>();
	}

	public void Initialize()
	{
		seed = UnityEngine.Random.Range(0, 1000000);
		UnityEngine.Random.InitState(seed);
		lines.Add(seed.ToString());
	}

    public void Load(string filename)
    {
        string text = File.ReadAllText(filename);
        lines = new List<string> (text.Split("\r\n", System.StringSplitOptions.RemoveEmptyEntries));
        index = 0;
		seed = int.Parse(lines[index++]);
		UnityEngine.Random.InitState(seed);
	}

	public void Save(string filename)
	{
		File.WriteAllLines(filename, lines);
		SendFile(filename);
	}

	public void Reset()
	{
		index = 0;
	}

	public void Clear()
	{
		Reset();
		lines.Clear();
	}

	public void Write()
	{
		lines.Add(Time.time.ToString());

		Add(player.position);
		//Add(playerBody.velocity);
		Add(player.rotation);
		//Add(playerBody.angularVelocity);

		Add(leftHand.position);
		//Add(leftBody.velocity);
		Add(leftHand.rotation);
		//Add(leftBody.angularVelocity);
		Add(leftGrabber.grabbing);

		Add(rightHand.position);
		//Add(rightBody.velocity);
		Add(rightHand.rotation);
		//Add(rightBody.angularVelocity);
		Add(rightGrabber.grabbing);
	}

	public bool Apply()
    {

        if (index >= lines.Count) return false;

        GetFloat();

		player.position = GetVector3();
		//playerBody.velocity = GetVector3();
		player.rotation = GetQuaternion();
		//playerBody.angularVelocity = GetVector3();

		leftHand.position = GetVector3();
		//leftBody.velocity = GetVector3();
		leftHand.rotation = GetQuaternion();
		//leftBody.angularVelocity = GetVector3();
        leftGrabber.SetGrabbing(GetBool());

		rightHand.position = GetVector3();
		//rightBody.velocity = GetVector3();
		rightHand.rotation = GetQuaternion();
		//rightBody.angularVelocity = GetVector3();
		rightGrabber.SetGrabbing(GetBool());

		if (index >= lines.Count) return false;

		return true;

    }

	void Add(float f)
	{
		lines.Add(f.ToString());
	}
	void Add(bool b)
	{
		Add(b ? 1f : 0f);
	}

	void Add(Vector3 v)
	{
		Add(v.x);
		Add(v.y);
		Add(v.z);
	}

	void Add(Quaternion q)
	{
		Add(q.x);
		Add(q.y);
		Add(q.z);
		Add(q.w);
	}

	float GetFloat()
	{
		float f = float.Parse(lines[index]);
		index++;
		return f;
	}

	bool GetBool()
	{
		float f = GetFloat();
		if (f == 0f) return false;
		if (f == 1f) return true;
		throw new InvalidDataException();
	}

	Vector3 GetVector3()
	{
		return new Vector3(GetFloat(), GetFloat(), GetFloat());
	}

	Quaternion GetQuaternion()
	{
		return new Quaternion(GetFloat(), GetFloat(), GetFloat(), GetFloat());
	}

	/*
	[MenuItem("Tools/Send Log")]
	static void SendFile()
	{
		SendFile("records/test.rcd");
	}
	*/

	static void SendFile(string filePath)
	{
		string serverIp = "192.168.1.10";
		int port = 63843;

		try
		{
			using (TcpClient client = new TcpClient(serverIp, port))
			{

				using (NetworkStream ns = client.GetStream())
				using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
				{

					byte[] buffer = new byte[1024];
					int bytesRead;

					byte[] secret = new byte[] {
						42,
						47
					};

					ns.Write(secret, 0, secret.Length);

					// Read from file and send to server
					while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
					{
						ns.Write(buffer, 0, bytesRead);
					}

					Console.WriteLine("Log sent successfully.");
				}
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Log : {ex.Message}");
		}
	}

}
