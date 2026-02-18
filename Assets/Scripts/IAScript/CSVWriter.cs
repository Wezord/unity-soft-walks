using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CSVWriter
{

	string filename;
	List<List<string>> observationData = new List<List<string>>();
	List<string> line = new List<string>();

	public CSVWriter(string filename)
	{
		this.filename = filename;
	}

	public void NextLine()
	{
		observationData.Add(line);
		line = new List<string>();
	}

	public void Add(string str)
	{
		line.Add(str);
	}

	public void Add(Vector3 vec)
	{
		line.Add(vec.magnitude.ToString());
		line.Add(vec.x.ToString());
		line.Add(vec.y.ToString());
		line.Add(vec.z.ToString());
	}

	public void AddVectorHeader(string name)
	{
		line.Add(name + " mag");
		line.Add(name + " x");
		line.Add(name + " y");
		line.Add(name + " z");
	}

	public void Write()
	{
		StreamWriter writer = new StreamWriter(filename, false);
		foreach (var list in observationData)
		{
			writer.WriteLine(string.Join(",", list));
		}
		writer.Close();
	}

}
