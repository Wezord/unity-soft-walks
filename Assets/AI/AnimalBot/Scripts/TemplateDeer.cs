using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using UnityEngine;

[ExecuteAlways]
public class TemplateDeer : MonoBehaviour{
    public GameObject root;
    public GameObject spine;
    public GameObject neck;

    public GameObject leftBackHip;
    public GameObject leftBackThigh;
    public GameObject leftBackShin;
    public GameObject leftBackFoot;

    public GameObject rightBackHip;
    public GameObject rightBackThigh;
    public GameObject rightBackShin;
    public GameObject rightBackFoot;

    public GameObject leftFrontClavicle;
    public GameObject leftFrontHip;
    public GameObject leftFrontThigh;
    public GameObject leftFrontShin;
    public GameObject leftFrontFoot;

    public GameObject rightFrontClavicle;
    public GameObject rightFrontHip;
    public GameObject rightFrontThigh;
    public GameObject rightFrontShin;
    public GameObject rightFrontFoot;

    public List<GameObject> GetJointsToCopy(){
        List<GameObject> res = new List<GameObject>
        {
            root,
            spine,
            neck,
            leftFrontClavicle,
            rightFrontClavicle,
            leftBackHip,
            rightBackHip,
            leftBackThigh,
            rightBackThigh,
            leftBackShin,
            rightBackShin,
            leftBackFoot,
            rightBackFoot,
            leftFrontHip,
            rightFrontHip,
            leftFrontThigh,
            rightFrontThigh,
            leftFrontShin,
            rightFrontShin,
            leftFrontFoot,
            rightFrontFoot
        };
        return res;
    }
}