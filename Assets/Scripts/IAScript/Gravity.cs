using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gravity : MonoBehaviour
{
    public float gravityFactor = 1f;

    void Start()
    {
        Physics.gravity *= gravityFactor;
    }
}
