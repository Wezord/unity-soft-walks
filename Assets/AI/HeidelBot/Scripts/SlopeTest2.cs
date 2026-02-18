using System;
using System.Collections;
using System.Collections.Generic;
// using System.Numerics;
using UnityEngine;

public class SlopeTest2 : MonoBehaviour
{
    public HumanBotAgent agent;
    public float iterPerSlope;
    public float maxDuration;
    public float initializationTime = 4;
    private float stepDuration;
    private float startTime;
    private float lastTime;
    private int countFrame = 0;
    private int iter = 1;
    private int step = 0;
    private float torque = 0f;
    private CSVWriter writer;
    private float[] slopes = {-9.22058824f, -8.45220588f, -7.68382353f, -6.91544118f, -6.14705882f, -5.37867647f, -4.61029412f,
     -3.84191176f, -3.07352941f, -2.30514706f, -1.53676471f, -0.76838235f, 0f, 0.76838235f, 1.53676471f, 2.30514706f, 3.07352941f, 
     3.84191176f, 4.61029412f, 5.37867647f, 6.14705882f, 6.91544118f, 7.68382353f, 8.45220588f, 9.22058824f};

    void Start()
    {
        Time.timeScale = 20;
        startTime = Time.fixedTime;
        lastTime = startTime;
        stepDuration = maxDuration - initializationTime;

        // Agent initial position
        agent.transform.position.Set(25, 0, 2);
        agent.transform.rotation.Set(0, 180, 0, 0);
   
        // Slope initial value
        transform.Rotate(Vector3.forward, slopes[0]);

        // Saving results
        string[] colNames = {"slope", "avg power(W) (" + stepDuration.ToString() + " s)"};
        InitWriter("slopTest2_data2.csv", colNames);
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
        if (step == slopes.Length)
        {
            writer.Write();
            Application.Quit();
            return;
        }

        countFrame++;
        float elapsed = Time.fixedTime - lastTime;

        if (elapsed >= maxDuration) 
        {
            iter++;
            agent.Restart();
            lastTime = Time.fixedTime;
        }
        else if (elapsed >= initializationTime) {
            // Vector3 velocity = (agent.transform.position - lastPos) / Time.fixedDeltaTime;
            // float speed = velocity.magnitude;
            torque += agent.GetConsumedEnergy();
            // lastPos = agent.transform.position;
        } 
        
        if (iter > iterPerSlope) 
        {
            writer.Add(slopes[step].ToString());
            writer.Add((torque / (iterPerSlope * stepDuration)).ToString());
            // writer.Add((torque / iterPerSlope * ).ToString());
            writer.NextLine();
            iter = 1;
            step++;
            countFrame = 0;
            torque = 0f;
            float diff = 0f;
            if (step != slopes.Length) diff = Mathf.Abs(slopes[step-1] - slopes[step]);
            transform.Rotate(Vector3.forward, diff);
            //transform.rotation.Set(0, 0, slopes[step], 0);
            // transform.SetPositionAndRotation(transform.position, new Quaternion(0, 0, slopes[step], 0));
            agent.Restart();
            lastTime = Time.fixedTime;      
        }
    }
}
