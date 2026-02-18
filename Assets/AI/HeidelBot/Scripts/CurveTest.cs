using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurveTest : MonoBehaviour
{
    public HumanBotAgent agent;
    public float maxDuration;
    private float startTime;
    private CSVWriter writer;

    void Start()
    {
        Time.timeScale = 20;
        startTime = Time.fixedTime;

        // Agent initial position
        agent.transform.position.Set(25, 0, 2);
        agent.transform.rotation.Set(0, 180, 0, 0);

        // Saving results
        string[] colNames = {"x", "y", "height", "torque(N.m)"};
        InitWriter("CurvTest_data1.csv", colNames);
    }

    void InitWriter(String filename, String[] colNames)
    {
        writer = new CSVWriter(filename);
        foreach (String s in colNames) writer.Add(s);
        writer.NextLine();
    }

    void FixedUpdate()
    {
        float elapsed = Time.fixedTime - startTime;
        if (elapsed >= maxDuration) 
        {
            writer.Write();
            Application.Quit();
            return;
        }
        else
        {
            writer.Add(agent.p.root.transform.position.z.ToString());
            writer.Add(agent.p.root.transform.position.x.ToString());
            writer.Add(agent.p.root.transform.position.y.ToString());
            writer.Add(agent.GetConsumedEnergy().ToString());
            writer.NextLine();
        }
    }
}
