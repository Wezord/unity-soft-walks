using System;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;


public class DeerAnimation : MonoBehaviour{

    public float speed = 1;

    private HumanBotParameters p;

    private
    void Start(){
        p = GetComponent<HumanBotParameters>();
    }

    void FixedUpdate(){
        if(p.animating)
            transform.position += speed*transform.forward*Time.fixedDeltaTime;
    }
}