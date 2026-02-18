using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlopeTest : MonoBehaviour
{
    public HumanBotAgent agent;
    public float minSlope;
    public float maxSlope;
    public float incr;
    public float iterPerSlope;
    public float maxDuration;
    public float initializationTime = 4;
    private float stepDuration;
    private float startTime;
    private float lastTime;
    private int iter = 1;
    private float step = 0;
    private float torque = 0f;
    private float distance = 0f;
    private Vector3 lastPos;
    private CSVWriter writer;

    void Start()
    {
        Time.timeScale = 20;
        startTime = Time.fixedTime;
        lastTime = startTime;
        stepDuration = maxDuration - initializationTime;

        // Agent initial position
        agent.transform.position.Set(25, 0, 2);
        lastPos = new Vector3(25, 0, 2);
        agent.transform.rotation.Set(0, 180, 0, 0);

        // Slope initial value
        transform.Rotate(Vector3.forward, minSlope);

        // Saving results
        string[] colNames = {"slope", "average torque(N.m/s) (" + stepDuration.ToString() + " s)", "distance", "average speed(m/s)"};
        InitWriter("slopTest_data1.csv", colNames);
    }

    void InitWriter(String filename, String[] colNames)
    {
        writer = new CSVWriter(filename);
        foreach (String s in colNames) writer.Add(s);
        writer.NextLine();
    }

    void FixedUpdate()
    {

        // If max slope is reached
        if (minSlope + incr * step > maxSlope)
        {
            writer.Write();
            Application.Quit();
            return;
        }  

        float elapsed = Time.fixedTime - lastTime;

        if (elapsed >= maxDuration) 
        {
            iter++;
            if (iter > iterPerSlope) 
            {
                writer.Add((minSlope + incr * step).ToString());
                writer.Add((torque / (iterPerSlope * stepDuration)).ToString());
                writer.Add(distance.ToString());
                writer.Add((distance / (iterPerSlope * stepDuration)).ToString());
                writer.NextLine();
                iter = 1;
                step++;
                torque = 0f;
                distance = 0f;
                transform.Rotate(Vector3.forward, incr);
            }
            agent.Restart();
            lastTime = Time.fixedTime;
            lastPos.Set(25, 0, 2);
        } else if (elapsed >= initializationTime) 
        {
            torque += agent.GetConsumedEnergy();
            distance += Mathf.Abs((lastPos - agent.transform.position).magnitude);
            lastPos = agent.transform.position;
        }      
    }
}
